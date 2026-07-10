using Vendex.Domain.Enums;

namespace Vendex.Domain.Entities;

public class Licenca : EntidadeBase
{
    public string CodigoInstalacao { get; set; } = string.Empty;
    public string? SerialAtivacao { get; set; }
    public DateTime? DataAtivacao { get; set; }
    public StatusLicenca Status { get; set; } = StatusLicenca.NaoAtivado;
}
