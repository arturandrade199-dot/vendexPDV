using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.Application.Services;
using Vendex.Domain.Enums;

namespace Vendex.App.ViewModels;

/// <summary>
/// Diálogo de autorização por login/senha — usado quando o operador logado não tem
/// permissão de exclusão para uma ação sensível (cancelar venda, remover item) e precisa
/// que alguém com permissão digite as próprias credenciais para liberar. Não troca a
/// SessaoUsuario: só confirma que o usuário informado existe e tem a permissão exigida.
/// </summary>
public partial class AutorizacaoWindowViewModel : ObservableObject
{
    private readonly IUsuarioService _usuarioService;
    private readonly string _nomeModulo;

    [ObservableProperty] private string mensagem;
    [ObservableProperty] private string login = string.Empty;
    [ObservableProperty] private string senha = string.Empty;
    [ObservableProperty] private string? mensagemErro;

    public event Action? Autorizado;

    public AutorizacaoWindowViewModel(IUsuarioService usuarioService, string nomeModulo, string mensagem)
    {
        _usuarioService = usuarioService;
        _nomeModulo = nomeModulo;
        this.mensagem = mensagem;
    }

    [RelayCommand]
    private async Task ConfirmarAsync()
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

        if (usuario.TipoUsuario != TipoUsuario.Administrador)
        {
            var permissoes = await _usuarioService.ObterPermissoesModulosAsync(usuario.Id);
            var podeExcluir = permissoes.FirstOrDefault(p => string.Equals(p.NomeModulo, _nomeModulo, StringComparison.OrdinalIgnoreCase))?.PodeExcluir ?? false;
            if (!podeExcluir)
            {
                MensagemErro = "Este usuário não tem permissão para autorizar esta ação.";
                return;
            }
        }

        MensagemErro = null;
        Autorizado?.Invoke();
    }
}
