using CommunityToolkit.Mvvm.ComponentModel;
using Vendex.Application.Services;
using Vendex.Domain.Entities;
using Vendex.Domain.Enums;

namespace Vendex.App.Navigation;

/// <summary>
/// Estado do usuário autenticado na sessão atual do app — singleton preenchido pelo
/// LoginWindow antes da MainWindow ser aberta. Substitui o hack de pegar o primeiro
/// usuário cadastrado (ver PdvViewModel/CaixaViewModel antes do módulo de Login existir).
/// </summary>
public partial class SessaoUsuario : ObservableObject
{
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EhAdministrador))]
    private Usuario? usuarioLogado;

    private Dictionary<string, PermissaoModulo> _permissoes = new(StringComparer.OrdinalIgnoreCase);

    public bool EhAdministrador => UsuarioLogado?.TipoUsuario == TipoUsuario.Administrador;

    /// <summary>Força a notificação de UsuarioLogado mesmo quando o objeto atualizado é a
    /// mesma referência (o DbContext é singleton e reaproveita a entidade já rastreada por
    /// Id) — sem isso, o setter gerado pelo [ObservableProperty] vê "mesma referência" e
    /// não dispara PropertyChanged, deixando o cabeçalho com nome/foto desatualizados até
    /// reiniciar o app.</summary>
    public void NotificarUsuarioAtualizado() => OnPropertyChanged(nameof(UsuarioLogado));

    public void DefinirPermissoes(IEnumerable<PermissaoModulo> permissoes) =>
        _permissoes = permissoes.ToDictionary(p => p.NomeModulo, StringComparer.OrdinalIgnoreCase);

    /// <summary>Administrador sempre tem acesso irrestrito; Funcionário só aos módulos liberados.</summary>
    public bool PodeAcessar(string nomeModulo) => EhAdministrador || (_permissoes.TryGetValue(nomeModulo, out var p) && p.PodeVisualizar);

    public bool PodeCriar(string nomeModulo) => EhAdministrador || (_permissoes.TryGetValue(nomeModulo, out var p) && p.PodeCriar);

    public bool PodeEditar(string nomeModulo) => EhAdministrador || (_permissoes.TryGetValue(nomeModulo, out var p) && p.PodeEditar);

    public bool PodeExcluir(string nomeModulo) => EhAdministrador || (_permissoes.TryGetValue(nomeModulo, out var p) && p.PodeExcluir);
}
