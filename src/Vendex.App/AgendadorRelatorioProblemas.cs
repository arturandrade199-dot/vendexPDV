using System.IO;
using System.Windows.Threading;
using Vendex.Application.Services;
using Vendex.Domain.Logging;

namespace Vendex.App;

/// <summary>Verifica 1x por dia se o log do dia anterior teve algum ERRO/AVISO e, nesse caso,
/// manda um relatório automático pro desenvolvedor. Não manda nada em dias sem ocorrência —
/// enviar todo dia mesmo sem problema viraria spam pra cada cliente que comprou o app.</summary>
public class AgendadorRelatorioProblemas
{
    private const string NomeArquivoMarcador = ".ultimo-envio-automatico";

    private readonly IRelatorioProblemaService _relatorioProblemaService;
    private readonly Dispatcher _dispatcherUi;
    private System.Threading.Timer? _timer;

    public AgendadorRelatorioProblemas(IRelatorioProblemaService relatorioProblemaService)
    {
        _relatorioProblemaService = relatorioProblemaService;

        // Mesmo motivo do AgendadorBackup/AgendadorLicenca: o Timer dispara numa thread do
        // ThreadPool, e os serviços resolvidos por DI aqui não são thread-safe.
        _dispatcherUi = Dispatcher.CurrentDispatcher;
    }

    public void Iniciar()
    {
        // Checagem "catch-up" imediata, igual ao AgendadorBackup — cobre o caso do app ter
        // sido aberto num dia depois de o marcador já ter ficado desatualizado. Repete de
        // hora em hora (não só 1x/24h) pra não depender do app ficar aberto num horário fixo.
        _ = VerificarEEnviarAsync();
        _timer = new System.Threading.Timer(
            _ => _dispatcherUi.InvokeAsync(VerificarEEnviarAsync),
            null, TimeSpan.FromHours(1), TimeSpan.FromHours(1));
    }

    private async Task VerificarEEnviarAsync()
    {
        try
        {
            var caminhoMarcador = Path.Combine(AppPaths.PastaLogs, NomeArquivoMarcador);
            var ultimoEnvio = File.Exists(caminhoMarcador) && DateTime.TryParse(File.ReadAllText(caminhoMarcador), out var data)
                ? data.Date
                : DateTime.MinValue;

            if (ultimoEnvio >= DateTime.Today)
                return;

            var ontem = DateTime.Today.AddDays(-1);
            var caminhoLogOntem = Path.Combine(AppPaths.PastaLogs, $"log-{ontem:yyyy-MM-dd}.txt");

            if (File.Exists(caminhoLogOntem))
            {
                var conteudo = File.ReadAllText(caminhoLogOntem);
                if (conteudo.Contains("[ERRO]") || conteudo.Contains("[AVISO]"))
                    await _relatorioProblemaService.EnviarAsync("automatico", null, conteudo);
            }

            Directory.CreateDirectory(AppPaths.PastaLogs);
            File.WriteAllText(caminhoMarcador, DateTime.Today.ToString("yyyy-MM-dd"));
        }
        catch (Exception ex)
        {
            Logger.Error("Falha na checagem/envio do relatório automático de problemas.", ex);
        }
    }
}
