using System.Windows;
using System.Windows.Input;
using Vendex.App.ViewModels;
using Vendex.Application.Services;
using Wpf.Ui.Controls;

namespace Vendex.App;

public partial class SelecionarProdutoWindow : FluentWindow
{
    private readonly SelecionarProdutoWindowViewModel _viewModel;

    public SelecionarProdutoWindow(SelecionarProdutoWindowViewModel viewModel)
    {
        InitializeComponent();
        this.ConfigurarComoDialogo();
        _viewModel = viewModel;
        DataContext = viewModel;
        viewModel.Confirmado += () =>
        {
            DialogResult = true;
            Close();
        };
    }

    public ResultadoBuscaProduto? ProdutoSelecionado => _viewModel.ProdutoSelecionado;

    private void BtnCancelar_Click(object sender, RoutedEventArgs e) => Close();

    private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) =>
        _viewModel.ConfirmarCommand.Execute(null);
}
