using System.Windows;
using Vendex.App.ViewModels;
using Wpf.Ui.Controls;

namespace Vendex.App;

public partial class UsuarioWindow : FluentWindow
{
    public UsuarioWindow(UsuarioWindowViewModel viewModel)
    {
        InitializeComponent();
        this.ConfigurarComoDialogo();

        // A lista de módulos cresce (mais módulos, mais colunas de permissão) e pode passar
        // da altura fixa em telas menores ou com escala de DPI mais alta — sem isso, o
        // Windows posiciona a janela parcialmente fora da tela em vez de deixar o
        // ScrollViewer interno assumir a rolagem.
        MaxHeight = SystemParameters.WorkArea.Height - 40;

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
