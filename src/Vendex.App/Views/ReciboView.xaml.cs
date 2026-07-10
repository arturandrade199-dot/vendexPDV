using System.Windows.Controls;

namespace Vendex.App.Views;

public partial class ReciboView : UserControl
{
    public ReciboView()
    {
        InitializeComponent();
    }

    private void Imprimir_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var dialogoImpressao = new PrintDialog();
        if (dialogoImpressao.ShowDialog() == true)
        {
            dialogoImpressao.PrintVisual(ReciboParaImprimir, "Cupom Vendex");
        }
    }
}
