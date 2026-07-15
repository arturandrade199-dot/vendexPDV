namespace Vendex.Application.Services;

public record ResumoFechamentoCaixa(
    DateTime DataAbertura,
    DateTime DataFechamento,
    decimal ValorAberturaTotal,
    decimal ValorFechamentoTotal,
    decimal EsperadoEmCaixa,
    decimal Divergencia,
    decimal FaturamentoTotal,
    decimal CustoTotal,
    decimal LucroTotal);
