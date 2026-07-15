using CommunityToolkit.Mvvm.ComponentModel;
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

    private HashSet<string> _modulosPermitidos = new(StringComparer.OrdinalIgnoreCase);

    public bool EhAdministrador => UsuarioLogado?.TipoUsuario == TipoUsuario.Administrador;

    public void DefinirModulosPermitidos(IEnumerable<string> nomesModulos) =>
        _modulosPermitidos = new HashSet<string>(nomesModulos, StringComparer.OrdinalIgnoreCase);

    /// <summary>Administrador sempre tem acesso irrestrito; Funcionário só aos módulos liberados.</summary>
    public bool PodeAcessar(string nomeModulo) => EhAdministrador || _modulosPermitidos.Contains(nomeModulo);
}
