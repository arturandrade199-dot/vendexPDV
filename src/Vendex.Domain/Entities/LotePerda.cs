namespace Vendex.Domain.Entities;

public class LotePerda : EntidadeBase
{
    public int LoteId { get; set; }
    public Lote Lote { get; set; } = null!;

    public DateTime DataHora { get; set; } = DateTime.Now;
    public decimal Quantidade { get; set; }
    public string? Motivo { get; set; }

    // Snapshot no momento da perda — se o PrecoCusto do produto mudar depois, o histórico do
    // relatório de perdas não pode mudar de valor silenciosamente.
    public decimal PrecoCustoUnitarioNaData { get; set; }
    public decimal ValorPerdido { get; set; }
}
