using Vendex.App.ViewModels;
using Wpf.Ui.Controls;

namespace Vendex.App;

public partial class ReciboWindow : FluentWindow
{
    public ReciboWindow(ReciboVenda recibo)
    {
        InitializeComponent();
        this.ConfigurarComoDialogo();
        DataContext = recibo;
    }
}
