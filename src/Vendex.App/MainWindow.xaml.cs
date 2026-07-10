using Vendex.App.Navigation;
using Wpf.Ui.Controls;

namespace Vendex.App;

public partial class MainWindow : FluentWindow
{
    public MainWindow(ShellViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
    }
}
