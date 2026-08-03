using System.Windows;
using System.Windows.Input;
using Vendex.App.ViewModels;
using Vendex.Domain.Entities;
using Wpf.Ui.Controls;

namespace Vendex.App;

public partial class SelecionarVendaWindow : FluentWindow
{
    private readonly SelecionarVendaWindowViewModel _viewModel;

    public SelecionarVendaWindow(SelecionarVendaWindowViewModel viewModel)
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

    public Venda? VendaSelecionada => _viewModel.VendaSelecionada;

    private void BtnCancelar_Click(object sender, RoutedEventArgs e) => Close();

    private void ListViewItem_MouseDoubleClick(object sender, MouseButtonEventArgs e) =>
        _viewModel.ConfirmarCommand.Execute(null);
}
