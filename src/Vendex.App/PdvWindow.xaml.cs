using Vendex.Application.Services;
using Vendex.App.ViewModels;
using Wpf.Ui.Controls;

namespace Vendex.App;

public partial class PdvWindow : FluentWindow
{
    public PdvWindow(PdvViewModel viewModel, IConfiguracaoImpressaoService configuracaoImpressaoService)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += (_, _) => TxtBusca.Focus();

        viewModel.VendaFinalizada += async _ =>
        {
            var configuracao = await configuracaoImpressaoService.ObterConfiguracaoAsync();
            if (configuracao.ImprimirVenda)
                ReciboControl.ImprimirAutomaticamente(configuracao.ImpressoraPadrao);
        };
    }
}
