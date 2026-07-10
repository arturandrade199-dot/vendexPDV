using Vendex.Domain.Enums;

namespace Vendex.Domain.Entities;

public class ContaPagarPagamento : EntidadeBase
{
    public int ContaPagarId { get; set; }
    public ContaPagar ContaPagar { get; set; } = null!;

    public decimal ValorPago { get; set; }
    public DateTime DataPagamento { get; set; } = DateTime.Now;
    public FormaPagamento FormaPagamento { get; set; }
}
