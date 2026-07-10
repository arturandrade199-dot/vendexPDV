namespace Vendex.Application.Services;

public record ResumoContasPagar(
    decimal Vencidos,
    decimal VencemHoje,
    decimal AVencer,
    decimal Pagos,
    decimal TotalPeriodo);
