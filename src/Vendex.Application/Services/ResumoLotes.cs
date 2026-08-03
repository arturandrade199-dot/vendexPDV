namespace Vendex.Application.Services;

public record ResumoLotes(
    int Vencidos,
    int VencemEm3Dias,
    int VencemEm7Dias,
    decimal PerdidoNoPeriodo);
