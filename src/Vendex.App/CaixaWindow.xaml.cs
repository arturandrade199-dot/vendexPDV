using System.Windows.Controls;
using Vendex.App.Impressao;
using Vendex.App.ViewModels;
using Vendex.Application.Services;
using Wpf.Ui.Controls;

namespace Vendex.App;

public partial class CaixaWindow : FluentWindow
{
    public CaixaWindow(CaixaViewModel viewModel, IConfiguracaoImpressaoService configuracaoImpressaoService)
    {
        InitializeComponent();
        this.ConfigurarComoDialogo();
        DataContext = viewModel;
        viewModel.Concluido += () =>
        {
            DialogResult = true;
            Close();
        };

        viewModel.PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName != nameof(CaixaViewModel.Estado))
                return;

            if (viewModel.Estado == CaixaViewModel.EstadoTela.AbrirRecibo ||
                viewModel.Estado == CaixaViewModel.EstadoTela.FecharRecibo)
            {
                var configuracao = await configuracaoImpressaoService.ObterConfiguracaoAsync();

                if (viewModel.Estado == CaixaViewModel.EstadoTela.AbrirRecibo && configuracao.ImprimirAberturaCaixa)
                    ImpressaoHelper.Imprimir(ReciboAberturaParaImprimir, "Abertura de Caixa", configuracao.ImpressoraPadrao);
                else if (viewModel.Estado == CaixaViewModel.EstadoTela.FecharRecibo && configuracao.ImprimirFechamentoCaixa)
                    ImpressaoHelper.Imprimir(ReciboFechamentoParaImprimir, "Fechamento de Caixa", configuracao.ImpressoraPadrao);
            }
        };
    }

    private void ImprimirAbertura_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var dialogoImpressao = new PrintDialog();
        if (dialogoImpressao.ShowDialog() == true)
        {
            dialogoImpressao.PrintVisual(ReciboAberturaParaImprimir, "Abertura de Caixa");
        }
    }

    private void ImprimirFechamento_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var dialogoImpressao = new PrintDialog();
        if (dialogoImpressao.ShowDialog() == true)
        {
            dialogoImpressao.PrintVisual(ReciboFechamentoParaImprimir, "Fechamento de Caixa");
        }
    }
}
