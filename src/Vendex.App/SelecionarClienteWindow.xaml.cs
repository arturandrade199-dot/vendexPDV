using System.Windows;
using System.Windows.Input;
using Vendex.App.ViewModels;
using Vendex.Domain.Entities;
using Wpf.Ui.Controls;

namespace Vendex.App;

public partial class SelecionarClienteWindow : FluentWindow
{
    private readonly SelecionarClienteWindowViewModel _viewModel;

    public SelecionarClienteWindow(SelecionarClienteWindowViewModel viewModel)
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

    public Cliente? ClienteSelecionado => _viewModel.ClienteSelecionado;

    private void BtnCancelar_Click(object sender, RoutedEventArgs e) => Close();

    private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) =>
        _viewModel.ConfirmarCommand.Execute(null);
}
