using System.Windows.Controls;
using Vendex.App.ViewModels;
using Wpf.Ui.Controls;

namespace Vendex.App;

public partial class CaixaWindow : FluentWindow
{
    public CaixaWindow(CaixaViewModel viewModel)
    {
        InitializeComponent();
        this.ConfigurarComoDialogo();
        DataContext = viewModel;
        viewModel.Concluido += () =>
        {
            DialogResult = true;
            Close();
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
