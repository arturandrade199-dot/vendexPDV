using Vendex.App.ViewModels;
using Wpf.Ui.Controls;

namespace Vendex.App;

public partial class UsuarioWindow : FluentWindow
{
    public UsuarioWindow(UsuarioWindowViewModel viewModel)
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

    private void PasswordBoxSenha_PasswordChanged(object sender, System.Windows.RoutedEventArgs e)
    {
        if (DataContext is UsuarioWindowViewModel viewModel)
            viewModel.Senha = PasswordBoxSenha.Password;
    }
}
