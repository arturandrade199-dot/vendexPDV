using Vendex.Domain.Enums;

namespace Vendex.Domain.Entities;

public class Lote : EntidadeBase
{
    public int ProdutoId { get; set; }
    public Produto Produto { get; set; } = null!;

    public int? ProdutoVarianteId { get; set; }
    public ProdutoVariante? ProdutoVariante { get; set; }

    public DateTime DataFabricacao { get; set; }
    public DateTime DataValidade { get; set; }
    public decimal QuantidadeInicial { get; set; }
    public decimal QuantidadeAtual { get; set; }
    public string? Observacoes { get; set; }
    public DateTime DataCadastro { get; set; } = DateTime.Now;

    // Computado, não persistido — mesmo raciocínio de ContaPagar.StatusEfetivo: a situação é
    // função de "hoje", não um estado que precisa de um job pra ficar em dia.
    public SituacaoLote SituacaoEfetiva(int diasAlertaVencimento = 7) =>
        QuantidadeAtual <= 0 ? SituacaoLote.Esgotado
        : DataValidade.Date < DateTime.Today ? SituacaoLote.Vencido
        : DataValidade.Date <= DateTime.Today.AddDays(diasAlertaVencimento) ? SituacaoLote.Vencendo
        : SituacaoLote.Valido;

    public ICollection<LotePerda> Perdas { get; set; } = new List<LotePerda>();
}
