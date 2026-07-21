using System.Windows.Threading;
using Vendex.Application.Services;
using Vendex.Domain.Logging;

namespace Vendex.App;

/// <summary>Verifica a cada minuto se é hora do backup automático configurado. Roda enquanto
/// o app estiver aberto (não existe serviço do Windows em segundo plano) — por isso também
/// faz uma checagem "catch-up" ao iniciar, cobrindo o caso do app ter sido aberto depois do
/// horário configurado.</summary>
public class AgendadorBackup
{
    private readonly IBackupService _backupService;
    private readonly Dispatcher _dispatcherUi;
    private System.Threading.Timer? _timer;

    public AgendadorBackup(IBackupService backupService)
    {
        _backupService = backupService;

        // System.Threading.Timer dispara numa thread do ThreadPool, não na thread de UI. O
        // VendexDbContext é singleton e não é thread-safe — sem marshalar de volta pra UI,
        // uma checagem do timer batendo com qualquer tela usando o banco na hora certa
        // pode estourar exceção (ou pior, corromper estado). Capturado aqui porque
        // AgendadorBackup é construído pelo DI na thread de UI, na inicialização do app.
        _dispatcherUi = Dispatcher.CurrentDispatcher;
    }

    public void Iniciar()
    {
        _ = VerificarEExecutarAsync();
        _timer = new System.Threading.Timer(
            _ => _dispatcherUi.InvokeAsync(VerificarEExecutarAsync),
            null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    private async Task VerificarEExecutarAsync()
    {
        try
        {
            var configuracao = await _backupService.ObterConfiguracaoAsync();
            if (!configuracao.Ativo || string.IsNullOrWhiteSpace(configuracao.CaminhoDestino))
                return;

            var agora = DateTime.Now;
            if (configuracao.UltimoBackupData?.Date == agora.Date || agora.TimeOfDay < configuracao.Horario)
                return;

            await _backupService.ExecutarBackupAsync(AppPaths.PastaDados);
        }
        catch (Exception ex)
        {
            // A falha já fica registrada em ConfiguracaoBackup (UltimoBackupSucesso/UltimaMensagemErro)
            // dentro do próprio ExecutarBackupAsync — uma falha de backup automático não pode
            // derrubar o app nem interromper o uso normal do PDV. ExecutarBackupAsync já loga
            // o detalhe; aqui cobre falhas fora dele (ex.: ObterConfiguracaoAsync).
            Logger.Error("Falha na checagem/execução do backup automático.", ex);
        }
    }
}
