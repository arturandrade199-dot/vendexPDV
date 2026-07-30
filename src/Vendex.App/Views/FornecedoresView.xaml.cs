using System.Windows.Controls;

namespace Vendex.App.Views;

public partial class FornecedoresView : UserControl
{
    public FornecedoresView()
    {
        InitializeComponent();
    }

    private void BtnAtalhos_Click(object sender, System.Windows.RoutedEventArgs e) =>
        PopupAtalhos.IsOpen = !PopupAtalhos.IsOpen;
}
