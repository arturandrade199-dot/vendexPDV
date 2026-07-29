using Vendex.Domain.Enums;

namespace Vendex.Domain.Entities;

public class Produto : EntidadeBase
{
    public string Nome { get; set; } = string.Empty;
    public string? CodigoBarras { get; set; }
    public decimal PrecoCusto { get; set; }
    public decimal PrecoVenda { get; set; }
    public decimal EstoqueAtual { get; set; }
    public bool Ativo { get; set; } = true;
    public UnidadeMedida UnidadeMedida { get; set; } = UnidadeMedida.UN;

    /// <summary>Quando true, o estoque real é a soma de <see cref="Variantes"/> — EstoqueAtual
    /// fica sempre zerado para não existir um número paralelo às variantes.</summary>
    public bool TemGrade { get; set; }
    public ICollection<ProdutoVariante> Variantes { get; set; } = new List<ProdutoVariante>();
}
