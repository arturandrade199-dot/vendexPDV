using Vendex.Domain.Entities;
using Vendex.Domain.Enums;
using Vendex.Domain.Interfaces;

namespace Vendex.Application.Services;

public class VendaService : IVendaService
{
    private readonly IUnitOfWork _unitOfWork;

    public VendaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<ResultadoBuscaProduto>> BuscarProdutosAsync(string termo)
    {
        var produtos = (await _unitOfWork.Produtos.ObterTodosAsync()).Where(p => p.Ativo).ToList();

        // Um código de barras é um identificador exato: se o termo bate em cheio com algum
        // código (do produto ou de uma variante), essa é a intenção do operador (leitor de
        // código de barras ou digitação do código) e teve prioridade sobre busca por nome —
        // senão um código curto (ex.: "2") acaba batendo por Contains() num nome de outro
        // produto que só por coincidência tem esse dígito (ex.: "coca cola 250ml").
        var correspondenciasExatas = new List<ResultadoBuscaProduto>();

        foreach (var produto in produtos)
        {
            if (!produto.TemGrade)
            {
                if (produto.CodigoBarras is not null && produto.CodigoBarras.Equals(termo, StringComparison.OrdinalIgnoreCase))
                {
                    correspondenciasExatas.Add(new ResultadoBuscaProduto(
                        produto.Id, produto.Nome, null, null, produto.CodigoBarras,
                        produto.PrecoVenda, produto.PrecoCusto, produto.EstoqueAtual));
                }

                continue;
            }

            var variante = produto.Variantes.FirstOrDefault(v =>
                v.CodigoBarras is not null && v.CodigoBarras.Equals(termo, StringComparison.OrdinalIgnoreCase));

            if (variante is not null)
            {
                correspondenciasExatas.Add(new ResultadoBuscaProduto(
                    produto.Id, produto.Nome, variante.Id, variante.Nome, variante.CodigoBarras,
                    produto.PrecoVenda, produto.PrecoCusto, variante.EstoqueAtual));
            }
        }

        if (correspondenciasExatas.Count > 0)
            return correspondenciasExatas.OrderBy(r => r.NomeExibicao).ToList();

        var resultados = new List<ResultadoBuscaProduto>();

        foreach (var produto in produtos)
        {
            if (!produto.TemGrade)
            {
                var bateNomeOuCodigo = produto.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                    (produto.CodigoBarras is not null && produto.CodigoBarras.Contains(termo, StringComparison.OrdinalIgnoreCase));

                if (bateNomeOuCodigo)
                {
                    resultados.Add(new ResultadoBuscaProduto(
                        produto.Id, produto.Nome, null, null, produto.CodigoBarras,
                        produto.PrecoVenda, produto.PrecoCusto, produto.EstoqueAtual));
                }

                continue;
            }

            var variantesPorCodigo = produto.Variantes
                .Where(v => v.CodigoBarras is not null && v.CodigoBarras.Contains(termo, StringComparison.OrdinalIgnoreCase))
                .ToList();

            if (variantesPorCodigo.Count > 0)
            {
                resultados.AddRange(variantesPorCodigo.Select(v => new ResultadoBuscaProduto(
                    produto.Id, produto.Nome, v.Id, v.Nome, v.CodigoBarras,
                    produto.PrecoVenda, produto.PrecoCusto, v.EstoqueAtual)));
                continue;
            }

            // Nome do produto bate: mostra todas as variações. Só o nome de uma variante
            // específica bate (ex.: buscar "M"): mostra só essa variante, não a grade inteira.
            if (produto.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase))
            {
                resultados.AddRange(produto.Variantes.Select(v => new ResultadoBuscaProduto(
                    produto.Id, produto.Nome, v.Id, v.Nome, v.CodigoBarras,
                    produto.PrecoVenda, produto.PrecoCusto, v.EstoqueAtual)));
                continue;
            }

            var variantesPorNome = produto.Variantes.Where(v => v.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase));
            resultados.AddRange(variantesPorNome.Select(v => new ResultadoBuscaProduto(
                produto.Id, produto.Nome, v.Id, v.Nome, v.CodigoBarras,
                produto.PrecoVenda, produto.PrecoCusto, v.EstoqueAtual)));
        }

        return resultados.OrderBy(r => r.NomeExibicao).ToList();
    }

    public async Task<Venda> FinalizarVendaAsync(
        IReadOnlyList<ItemCarrinho> itens,
        IReadOnlyList<PagamentoAplicado> pagamentos,
        int usuarioId,
        int? clienteId = null,
        DateTime? vencimentoFiado = null)
    {
        if (pagamentos.Count == 0)
            throw new InvalidOperationException("Informe ao menos uma forma de pagamento.");

        var totalVenda = itens.Sum(i => i.PrecoUnitario * i.Quantidade);
        var totalPago = pagamentos.Sum(p => p.Valor);
        if (totalPago != totalVenda)
            throw new InvalidOperationException("A soma dos pagamentos não bate com o total da venda.");

        var pagamentoFiado = pagamentos.FirstOrDefault(p => p.FormaPagamento == FormaPagamento.Fiado);
        if (pagamentoFiado is not null && clienteId is null)
            throw new InvalidOperationException("Venda fiado exige um cliente.");

        var venda = new Venda
        {
            DataHora = DateTime.Now,
            UsuarioId = usuarioId,
            ClienteId = clienteId,
            ValorTotal = totalVenda
        };

        foreach (var item in itens)
        {
            venda.Itens.Add(new VendaItem
            {
                ProdutoId = item.ProdutoId,
                ProdutoVarianteId = item.ProdutoVarianteId,
                Quantidade = item.Quantidade,
                PrecoUnitario = item.PrecoUnitario,
                PrecoCustoUnitario = item.PrecoCustoUnitario,
                Subtotal = item.PrecoUnitario * item.Quantidade
            });

            var produto = await _unitOfWork.Produtos.ObterPorIdAsync(item.ProdutoId);
            if (produto is not null)
            {
                if (item.ProdutoVarianteId is int varianteId)
                {
                    var variante = produto.Variantes.FirstOrDefault(v => v.Id == varianteId);
                    if (variante is not null)
                        variante.EstoqueAtual -= item.Quantidade;
                }
                else
                {
                    produto.EstoqueAtual -= item.Quantidade;
                }

                _unitOfWork.Produtos.Atualizar(produto);
            }
        }

        foreach (var pagamento in pagamentos)
        {
            venda.Pagamentos.Add(new VendaPagamento
            {
                FormaPagamento = pagamento.FormaPagamento,
                Valor = pagamento.Valor
            });
        }

        await _unitOfWork.Vendas.AdicionarAsync(venda);

        if (pagamentoFiado is not null)
        {
            var contaReceber = new ContaReceber
            {
                ClienteId = clienteId!.Value,
                Venda = venda,
                Descricao = "Venda fiado (PDV)",
                ValorTotal = pagamentoFiado.Valor,
                DataLancamento = DateTime.Now,
                DataVencimento = vencimentoFiado ?? DateTime.Today.AddDays(30),
                Status = StatusContaFinanceira.Aberto
            };

            await _unitOfWork.ContasReceber.AdicionarAsync(contaReceber);
        }

        await _unitOfWork.SalvarAlteracoesAsync();
        return venda;
    }

    public Task<IReadOnlyList<Venda>> ListarPorPeriodoAsync(DateTime inicio, DateTime fim) =>
        _unitOfWork.Vendas.ObterPorPeriodoAsync(inicio, fim);

    public Task<Venda?> ObterPorIdAsync(int id) =>
        _unitOfWork.Vendas.ObterComItensAsync(id);
}
