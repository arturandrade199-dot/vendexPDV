using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.App.Navigation;
using Vendex.Application.Services;
using Vendex.Domain.Enums;

namespace Vendex.App.ViewModels;

public partial class LoginViewModel : ObservableObject
{
    private readonly IUsuarioService _usuarioService;
    private readonly SessaoUsuario _sessao;

    [ObservableProperty] private bool modoCriarPrimeiroAdministrador;
    [ObservableProperty] private string nome = string.Empty;
    [ObservableProperty] private string login = string.Empty;
    [ObservableProperty] private string senha = string.Empty;
    [ObservableProperty] private string confirmarSenha = string.Empty;
    [ObservableProperty] private string? mensagemErro;

    public event Action? Autenticado;

    public LoginViewModel(IUsuarioService usuarioService, SessaoUsuario sessao)
    {
        _usuarioService = usuarioService;
        _sessao = sessao;
        _ = VerificarPrimeiroAcessoAsync();
    }

    private async Task VerificarPrimeiroAcessoAsync()
    {
        ModoCriarPrimeiroAdministrador = !await _usuarioService.ExisteAlgumUsuarioAsync();
    }

    [RelayCommand]
    private async Task EntrarAsync()
    {
        if (string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Senha))
        {
            MensagemErro = "Informe login e senha.";
            return;
        }

        var usuario = await _usuarioService.ValidarLoginAsync(Login.Trim(), Senha);
        if (usuario is null)
        {
            MensagemErro = "Login ou senha inválidos.";
            return;
        }

        MensagemErro = null;
        _sessao.UsuarioLogado = usuario;
        var modulosPermitidos = await _usuarioService.ObterNomesModulosPermitidosAsync(usuario.Id);
        _sessao.DefinirModulosPermitidos(modulosPermitidos);
        Autenticado?.Invoke();
    }

    [RelayCommand]
    private async Task CriarAdministradorAsync()
    {
        if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Login) || string.IsNullOrWhiteSpace(Senha))
        {
            MensagemErro = "Preencha nome, login e senha.";
            return;
        }

        if (Senha.Length < 4)
        {
            MensagemErro = "A senha deve ter ao menos 4 caracteres.";
            return;
        }

        if (Senha != ConfirmarSenha)
        {
            MensagemErro = "As senhas não coincidem.";
            return;
        }

        try
        {
            var usuario = await _usuarioService.CriarUsuarioAsync(Nome.Trim(), Login.Trim(), Senha, TipoUsuario.Administrador);
            MensagemErro = null;
            _sessao.UsuarioLogado = usuario;
            Autenticado?.Invoke();
        }
        catch (InvalidOperationException ex)
        {
            MensagemErro = ex.Message;
        }
    }
}
