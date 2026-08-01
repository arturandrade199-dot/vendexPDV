using Vendex.App.ViewModels;
using Wpf.Ui.Controls;

namespace Vendex.App;

public partial class DevolucaoWindow : FluentWindow
{
    public DevolucaoWindow(DevolucaoWindowViewModel viewModel)
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

    private void BtnCancelar_Click(object sender, System.Windows.RoutedEventArgs e) => Close();
}
