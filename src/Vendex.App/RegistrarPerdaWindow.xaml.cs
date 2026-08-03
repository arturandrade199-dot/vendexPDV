using Vendex.App.ViewModels;
using Wpf.Ui.Controls;

namespace Vendex.App;

public partial class RegistrarPerdaWindow : FluentWindow
{
    public RegistrarPerdaWindow(RegistrarPerdaWindowViewModel viewModel)
    {
        InitializeComponent();
        this.ConfigurarComoDialogo();
        DataContext = viewModel;
        viewModel.Confirmado += () =>
        {
            DialogResult = true;
            Close();
        };
    }

    private void BtnCancelar_Click(object sender, System.Windows.RoutedEventArgs e) => Close();
}
