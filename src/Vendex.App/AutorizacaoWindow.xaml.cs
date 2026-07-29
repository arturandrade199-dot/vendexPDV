using Vendex.App.ViewModels;
using Wpf.Ui.Controls;

namespace Vendex.App;

public partial class AutorizacaoWindow : FluentWindow
{
    public AutorizacaoWindow(AutorizacaoWindowViewModel viewModel)
    {
        InitializeComponent();
        this.ConfigurarComoDialogo();
        DataContext = viewModel;
        viewModel.Autorizado += () =>
        {
            DialogResult = true;
            Close();
        };
    }

    private void PasswordBoxSenha_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is AutorizacaoWindowViewModel viewModel)
            viewModel.Senha = PasswordBoxSenha.Password;
    }
}
