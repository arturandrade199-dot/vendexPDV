using Vendex.Domain.Entities;

namespace Vendex.Application.Services;

public interface IBackupService
{
    Task<ConfiguracaoBackup> ObterConfiguracaoAsync();
    Task SalvarConfiguracaoAsync(bool ativo, TimeSpan horario, string? caminhoDestino);
    Task<ConfiguracaoBackup> ExecutarBackupAsync(string caminhoPastaDados);
}
