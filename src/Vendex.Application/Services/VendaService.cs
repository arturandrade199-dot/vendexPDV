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

    public async Task<IReadOnlyList<Produto>> BuscarProdutosAsync(string termo)
    {
        var produtos = await _unitOfWork.Produtos.ObterTodosAsync();
        return produtos
            .Where(p => p.Ativo && (
                p.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                (p.CodigoBarras is not null && p.CodigoBarras.Contains(termo, StringComparison.OrdinalIgnoreCase))))
            .OrderBy(p => p.Nome)
            .ToList();
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
                Quantidade = item.Quantidade,
                PrecoUnitario = item.PrecoUnitario,
                PrecoCustoUnitario = item.PrecoCustoUnitario,
                Subtotal = item.PrecoUnitario * item.Quantidade
            });

            var produto = await _unitOfWork.Produtos.ObterPorIdAsync(item.ProdutoId);
            if (produto is not null)
            {
                produto.EstoqueAtual -= item.Quantidade;
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
