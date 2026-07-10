using Vendex.Domain.Enums;

namespace Vendex.Domain.Entities;

public class Venda : EntidadeBase
{
    public DateTime DataHora { get; set; } = DateTime.Now;

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    public decimal ValorTotal { get; set; }
    public bool Cancelada { get; set; }

    public ICollection<VendaItem> Itens { get; set; } = new List<VendaItem>();

    /// <summary>Uma ou mais formas de pagamento aplicadas à venda (pagamento misto).</summary>
    public ICollection<VendaPagamento> Pagamentos { get; set; } = new List<VendaPagamento>();
}
