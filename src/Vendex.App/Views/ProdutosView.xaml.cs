using System.Windows.Controls;

namespace Vendex.App.Views;

public partial class ProdutosView : UserControl
{
    public ProdutosView()
    {
        InitializeComponent();
    }

    private void BtnAtalhos_Click(object sender, System.Windows.RoutedEventArgs e) =>
        PopupAtalhos.IsOpen = !PopupAtalhos.IsOpen;
}
