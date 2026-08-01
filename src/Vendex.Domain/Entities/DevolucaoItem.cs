namespace Vendex.Domain.Entities;

public class DevolucaoItem : EntidadeBase
{
    public int DevolucaoId { get; set; }
    public Devolucao Devolucao { get; set; } = null!;

    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    public int? ProdutoVarianteId { get; set; }
    public ProdutoVariante? ProdutoVariante { get; set; }

    public decimal Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
