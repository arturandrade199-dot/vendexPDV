using Vendex.Domain.Enums;

namespace Vendex.Domain.Entities;

public class CaixaMovimentacao : EntidadeBase
{
    public int CaixaId { get; set; }
    public Caixa Caixa { get; set; } = null!;

    public TipoMovimentacaoCaixa Tipo { get; set; }
    public decimal Valor { get; set; }
    public string Motivo { get; set; } = string.Empty;
    public DateTime DataHora { get; set; } = DateTime.Now;
}
