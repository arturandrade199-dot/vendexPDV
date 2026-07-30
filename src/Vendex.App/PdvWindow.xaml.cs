using System.Windows.Input;
using Vendex.Application.Services;
using Vendex.App.Impressao;
using Vendex.App.ViewModels;
using Wpf.Ui.Controls;

namespace Vendex.App;

public partial class PdvWindow : FluentWindow
{
    // RoutedCommand em vez de um comando no ViewModel porque "imprimir" mexe direto com o
    // visual do cupom (ReciboControl) — é responsabilidade da View, não do PdvViewModel.
    public static readonly RoutedCommand ImprimirCupomCommand = new();

    public PdvWindow(PdvViewModel viewModel, IConfiguracaoImpressaoService configuracaoImpressaoService)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += (_, _) => TxtBusca.Focus();

        CommandBindings.Add(new CommandBinding(
            ImprimirCupomCommand,
            (_, _) => ReciboControl.Imprimir(),
            (_, e) => e.CanExecute = viewModel.EstaComRecibo));

        viewModel.VendaFinalizada += async _ =>
        {
            var configuracao = await configuracaoImpressaoService.ObterConfiguracaoAsync();

            // Configura mesmo quando ImprimirVenda é falso: o clique manual em "Imprimir
            // cupom" (ou Ctrl+P) pode acontecer bem depois disso, e precisa das mesmas
            // preferências de layout/dados da loja.
            ReciboControl.UsarLayoutTermico = configuracao.UsarLayoutTermico;
            ReciboControl.DadosLoja = new DadosLojaImpressao(configuracao.NomeLoja, configuracao.EnderecoLoja, configuracao.CnpjLoja);

            if (configuracao.ImprimirVenda)
                ReciboControl.ImprimirAutomaticamente(configuracao.ImpressoraPadrao);
        };

        viewModel.SairSolicitado += Close;
    }
}
