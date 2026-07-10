using Vendex.Domain.Entities;
using Vendex.Domain.Enums;

namespace Vendex.Application.Services;

public interface IUsuarioService
{
    Task<IReadOnlyList<Usuario>> ListarAsync();
    Task<bool> ExisteAlgumUsuarioAsync();
    Task<Usuario> CriarUsuarioAsync(string nome, string login, string senha, TipoUsuario tipoUsuario);
    Task<Usuario?> ValidarLoginAsync(string login, string senha);
}
