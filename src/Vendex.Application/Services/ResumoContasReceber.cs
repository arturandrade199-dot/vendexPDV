namespace Vendex.Application.Services;

public record ResumoContasReceber(
    decimal Vencidos,
    decimal VencemHoje,
    decimal AVencer,
    decimal Recebidos,
    decimal TotalPeriodo);
