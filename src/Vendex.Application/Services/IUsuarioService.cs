using Vendex.Domain.Entities;
using Vendex.Domain.Enums;

namespace Vendex.Application.Services;

public interface IUsuarioService
{
    Task<IReadOnlyList<Usuario>> ListarAsync();
    Task<bool> ExisteAlgumUsuarioAsync();
    Task<Usuario> CriarUsuarioAsync(string nome, string login, string senha, TipoUsuario tipoUsuario);
    Task AtualizarAsync(int usuarioId, string nome, string login, TipoUsuario tipoUsuario, string? novaSenha);
    Task<Usuario> AtualizarPerfilAsync(int usuarioId, string nome, string? fotoCaminho);
    Task AlternarAtivoAsync(int usuarioId);
    Task<Usuario?> ValidarLoginAsync(string login, string senha);

    Task<IReadOnlyList<Modulo>> ListarModulosAsync();
    Task<IReadOnlyList<PermissaoModulo>> ObterPermissoesModulosAsync(int usuarioId);
    Task DefinirPermissoesAsync(int usuarioId, IReadOnlyList<PermissaoModulo> permissoes);
}
