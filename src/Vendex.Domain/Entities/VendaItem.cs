namespace Vendex.Domain.Entities;

public class VendaItem : EntidadeBase
{
    public int VendaId { get; set; }
    public Venda Venda { get; set; } = null!;

    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    public int? ProdutoVarianteId { get; set; }
    public ProdutoVariante? ProdutoVariante { get; set; }

    public decimal Quantidade { get; set; }
    public decimal PrecoUnitario { get; set; }
    public decimal PrecoCustoUnitario { get; set; }
    public decimal Subtotal { get; set; }
}
