namespace Vendex.Domain.Entities;

public class ConfiguracaoBackup : EntidadeBase
{
    public bool Ativo { get; set; }
    public TimeSpan Horario { get; set; } = new TimeSpan(22, 0, 0);
    public string? CaminhoDestino { get; set; }
    public DateTime? UltimoBackupData { get; set; }
    public bool UltimoBackupSucesso { get; set; }
    public string? UltimaMensagemErro { get; set; }
}
