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
        var produtos = await _unitOfWork.Produtos.ObterTodosAsync();
        var resultados = new List<ResultadoBuscaProduto>();

        foreach (var produto in produtos.Where(p => p.Ativo))
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

            var variantePorCodigo = produto.Variantes.FirstOrDefault(v =>
                v.CodigoBarras is not null && v.CodigoBarras.Contains(termo, StringComparison.OrdinalIgnoreCase));

            if (variantePorCodigo is not null)
            {
                resultados.Add(new ResultadoBuscaProduto(
                    produto.Id, produto.Nome, variantePorCodigo.Id, variantePorCodigo.Nome, variantePorCodigo.CodigoBarras,
                    produto.PrecoVenda, produto.PrecoCusto, variantePorCodigo.EstoqueAtual));
                continue;
            }

            var bateNomeProdutoOuVariante = produto.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                produto.Variantes.Any(v => v.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase));

            if (bateNomeProdutoOuVariante)
            {
                resultados.AddRange(produto.Variantes.Select(v => new ResultadoBuscaProduto(
                    produto.Id, produto.Nome, v.Id, v.Nome, v.CodigoBarras,
                    produto.PrecoVenda, produto.PrecoCusto, v.EstoqueAtual)));
            }
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
}
