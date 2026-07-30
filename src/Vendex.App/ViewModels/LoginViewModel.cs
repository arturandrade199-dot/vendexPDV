using System.Linq;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.App.Navigation;
using Vendex.Application.Services;
using Vendex.Domain.Enums;

namespace Vendex.App.ViewModels;

public partial class LoginViewModel : ObservableObject
{
#if DEBUG
    // Bypass só para build de Debug — nunca compilado em Release, nunca vai pro cliente.
    // Loga como um usuário "Desenvolvedor" com permissão total, independente do que estiver
    // digitado no campo Login. Troque a senha abaixo por algo só seu se quiser.
    private const string LoginUsuarioDev = "admin";
    private const string SenhaDev = "admin";
#endif

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

#if DEBUG
        if (Senha == SenhaDev)
        {
            await EntrarComoDevAsync();
            return;
        }
#endif

        var usuario = await _usuarioService.ValidarLoginAsync(Login.Trim(), Senha);
        if (usuario is null)
        {
            MensagemErro = "Login ou senha inválidos.";
            return;
        }

        MensagemErro = null;
        _sessao.UsuarioLogado = usuario;
        var permissoes = await _usuarioService.ObterPermissoesModulosAsync(usuario.Id);
        _sessao.DefinirPermissoes(permissoes);
        Autenticado?.Invoke();
    }

#if DEBUG
    private async Task EntrarComoDevAsync()
    {
        var usuarios = await _usuarioService.ListarAsync();
        var usuarioDev = usuarios.FirstOrDefault(u => u.Login == LoginUsuarioDev)
            ?? await _usuarioService.CriarUsuarioAsync("Desenvolvedor", LoginUsuarioDev, Guid.NewGuid().ToString(), TipoUsuario.Administrador);

        MensagemErro = null;
        _sessao.UsuarioLogado = usuarioDev;
        var permissoes = await _usuarioService.ObterPermissoesModulosAsync(usuarioDev.Id);
        _sessao.DefinirPermissoes(permissoes);
        Autenticado?.Invoke();
    }
#endif

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
