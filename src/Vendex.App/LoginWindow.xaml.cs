using Vendex.App.ViewModels;
using Wpf.Ui.Controls;

namespace Vendex.App;

public partial class LoginWindow : FluentWindow
{
    public LoginWindow(LoginViewModel viewModel)
    {
        InitializeComponent();
        this.ConfigurarComoDialogo();
        DataContext = viewModel;
        viewModel.Autenticado += () =>
        {
            DialogResult = true;
            Close();
        };
    }

    private void PasswordBoxLogin_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel viewModel)
            viewModel.Senha = PasswordBoxLogin.Password;
    }

    private void PasswordBoxCriar_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel viewModel)
            viewModel.Senha = PasswordBoxCriar.Password;
    }

    private void PasswordBoxConfirmar_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is LoginViewModel viewModel)
            viewModel.ConfirmarSenha = PasswordBoxConfirmar.Password;
    }
}
