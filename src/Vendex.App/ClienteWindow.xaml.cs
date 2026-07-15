using Vendex.App.ViewModels;
using Wpf.Ui.Controls;

namespace Vendex.App;

public partial class ClienteWindow : FluentWindow
{
    public ClienteWindow(ClienteWindowViewModel viewModel)
    {
        InitializeComponent();
        this.ConfigurarComoDialogo();
        DataContext = viewModel;
        viewModel.Salvo += () =>
        {
            DialogResult = true;
            Close();
        };
    }
}
