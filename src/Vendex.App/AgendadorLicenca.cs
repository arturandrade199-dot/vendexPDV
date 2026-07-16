using System.Windows.Threading;
using Vendex.Application.Services;

namespace Vendex.App;

/// <summary>Reconfirma a licença a cada 24h enquanto o app está aberto, pra detectar
/// cancelamento de assinatura sem precisar reiniciar o app. A checagem de abertura já
/// acontece de forma bloqueante em App.xaml.cs antes da LoginWindow — este agendador só
/// cobre o caso de a assinatura ser cancelada no meio de uma sessão longa.</summary>
public class AgendadorLicenca
{
    private readonly ILicencaService _licencaService;
    private readonly Dispatcher _dispatcherUi;
    private System.Threading.Timer? _timer;

    public AgendadorLicenca(ILicencaService licencaService)
    {
        _licencaService = licencaService;

        // Mesmo motivo do AgendadorBackup: o Timer dispara numa thread do ThreadPool,
        // e o DbContext (singleton, usado por ILicencaService) não é thread-safe.
        _dispatcherUi = Dispatcher.CurrentDispatcher;
    }

    public void Iniciar()
    {
        _timer = new System.Threading.Timer(
            _ => _dispatcherUi.InvokeAsync(VerificarAsync),
            null, TimeSpan.FromHours(24), TimeSpan.FromHours(24));
    }

    private async Task VerificarAsync()
    {
        try
        {
            var liberado = await _licencaService.VerificarELiberarAsync();
            if (!liberado)
            {
                System.Windows.MessageBox.Show(
                    "Sua licença não está mais ativa. O Vendex PDV será encerrado.",
                    "Licença inválida",
                    System.Windows.MessageBoxButton.OK,
                    System.Windows.MessageBoxImage.Warning);
                System.Windows.Application.Current.Shutdown();
            }
        }
        catch
        {
            // Falha na checagem periódica não pode derrubar o app durante o uso normal
            // — só tenta de novo no próximo ciclo (ou na folga offline, se for o caso).
        }
    }
}
