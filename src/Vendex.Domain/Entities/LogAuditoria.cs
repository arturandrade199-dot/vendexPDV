using Vendex.Domain.Enums;

namespace Vendex.Domain.Entities;

public class LogAuditoria : EntidadeBase
{
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public DateTime DataHora { get; set; } = DateTime.Now;
    public string Modulo { get; set; } = string.Empty;
    public TipoAcaoAuditoria Acao { get; set; }
    public string Entidade { get; set; } = string.Empty;
    public int? EntidadeId { get; set; }
    public string Descricao { get; set; } = string.Empty;
}
