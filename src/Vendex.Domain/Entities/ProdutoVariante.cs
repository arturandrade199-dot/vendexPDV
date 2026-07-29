namespace Vendex.Domain.Entities;

public class ProdutoVariante : EntidadeBase
{
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    public string Nome { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }
    public decimal EstoqueAtual { get; set; }
}
