using Vendex.Domain.Enums;

namespace Vendex.Domain.Entities;

public class ContaReceberPagamento : EntidadeBase
{
    public int ContaReceberId { get; set; }
    public ContaReceber ContaReceber { get; set; } = null!;

    public decimal ValorPago { get; set; }
    public DateTime DataPagamento { get; set; } = DateTime.Now;
    public FormaPagamento FormaPagamento { get; set; }
}
