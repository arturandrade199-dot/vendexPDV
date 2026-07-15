using Vendex.App.ViewModels;
using Wpf.Ui.Controls;

namespace Vendex.App;

public partial class FornecedorWindow : FluentWindow
{
    public FornecedorWindow(FornecedorWindowViewModel viewModel)
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
