using Vendex.App.ViewModels;
using Wpf.Ui.Controls;

namespace Vendex.App;

public partial class PdvWindow : FluentWindow
{
    public PdvWindow(PdvViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        Loaded += (_, _) => TxtBusca.Focus();
    }
}
