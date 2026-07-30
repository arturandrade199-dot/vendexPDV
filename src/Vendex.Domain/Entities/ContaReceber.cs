using Vendex.Domain.Enums;

namespace Vendex.Domain.Entities;

public class ContaReceber : EntidadeBase
{
    public int ClienteId { get; set; }
    public Cliente Cliente { get; set; } = null!;

    public int? VendaId { get; set; }
    public Venda? Venda { get; set; }

    public string Descricao { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public DateTime DataLancamento { get; set; } = DateTime.Now;
    public DateTime DataVencimento { get; set; }
    public StatusContaFinanceira Status { get; set; } = StatusContaFinanceira.Aberto;

    // Nada nunca atribui StatusContaFinanceira.Atrasado a Status — ele é computado aqui em
    // vez de persistido, senão exigiria um job/checagem periódica só pra "envelhecer" contas.
    public StatusContaFinanceira StatusEfetivo =>
        Status != StatusContaFinanceira.Pago && DataVencimento.Date < DateTime.Today
            ? StatusContaFinanceira.Atrasado
            : Status;

    public ICollection<ContaReceberPagamento> Pagamentos { get; set; } = new List<ContaReceberPagamento>();
}
