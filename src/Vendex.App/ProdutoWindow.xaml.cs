using Vendex.App.ViewModels;
using Wpf.Ui.Controls;

namespace Vendex.App;

public partial class ProdutoWindow : FluentWindow
{
    public ProdutoWindow(ProdutoWindowViewModel viewModel)
    {
        InitializeComponent();
        DataContext = viewModel;
        viewModel.Salvo += () =>
        {
            DialogResult = true;
            Close();
        };
    }
}
