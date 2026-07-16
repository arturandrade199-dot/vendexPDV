using Vendex.Domain.Enums;

namespace Vendex.Domain.Entities;

public class Licenca : EntidadeBase
{
    public string CodigoInstalacao { get; set; } = string.Empty;
    public string? SerialAtivacao { get; set; }
    public DateTime? DataAtivacao { get; set; }
    public StatusLicenca Status { get; set; } = StatusLicenca.NaoAtivado;

    // Assinatura via Hotmart + Supabase (ver Vendex.Application.LicencaService).
    public string? Email { get; set; }
    public DateTime? DataValidaAte { get; set; }
    public DateTime? UltimaVerificacaoOnline { get; set; }

    // Só pra detectar relógio do Windows atrasado de propósito — nunca usado pra
    // calcular validade de licença, só como "a hora não pode ter voltado desde a
    // última vez que o app rodou".
    public DateTime UltimaDataVista { get; set; } = DateTime.Now;
}
