namespace Vendex.Application.Services;

public enum TipoRelatorio
{
    EstoqueProdutos,
    ProdutosMaisVendidos,
    ProdutosMenosVendidos,
    ContasPagas,
    ContasAPagar,
    ContasRecebidas,
    ContasAReceber,
    AberturasCaixa,
    FechamentosCaixa,
    VendasPorFormaPagamento
}

public static class RelatoriosDisponiveis
{
    public static readonly IReadOnlyList<(TipoRelatorio Tipo, string Nome, bool PrecisaPeriodo)> Todos = new[]
    {
        (TipoRelatorio.EstoqueProdutos, "Estoque de produtos", false),
        (TipoRelatorio.ProdutosMaisVendidos, "Produtos que mais venderam", true),
        (TipoRelatorio.ProdutosMenosVendidos, "Produtos com menos saída", true),
        (TipoRelatorio.ContasPagas, "Contas pagas no período", true),
        (TipoRelatorio.ContasAPagar, "Contas a pagar no período", true),
        (TipoRelatorio.ContasRecebidas, "Contas recebidas no período", true),
        (TipoRelatorio.ContasAReceber, "Contas a receber no período", true),
        (TipoRelatorio.AberturasCaixa, "Abertura de caixas no período", true),
        (TipoRelatorio.FechamentosCaixa, "Fechamento de caixa no período", true),
        (TipoRelatorio.VendasPorFormaPagamento, "Total de vendas por forma de pagamento", true),
    };
}
