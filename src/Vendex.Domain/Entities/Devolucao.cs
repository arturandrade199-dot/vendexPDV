namespace Vendex.Domain.Entities;

public class Devolucao : EntidadeBase
{
    public DateTime DataHora { get; set; } = DateTime.Now;

    /// <summary>Nulo quando a devolução é manual/avulsa e o operador não quer (ou não tem
    /// como) identificar o cliente — o vínculo com o cliente é só um registro histórico,
    /// nada no fluxo de devolução depende dele.</summary>
    public int? ClienteId { get; set; }
    public Cliente? Cliente { get; set; }

    /// <summary>Nulo quando a devolução é avulsa (sem vínculo com uma venda do histórico).</summary>
    public int? VendaId { get; set; }
    public Venda? Venda { get; set; }

    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public string? Motivo { get; set; }
    public decimal ValorTotal { get; set; }

    /// <summary>Se true, o valor devolvido saiu do caixa (lançado como uma Sangria) — ver
    /// DevolucaoService.RegistrarAsync. Se false, a devolução só afetou o estoque.</summary>
    public bool EstornouCaixa { get; set; }

    public ICollection<DevolucaoItem> Itens { get; set; } = new List<DevolucaoItem>();
}
