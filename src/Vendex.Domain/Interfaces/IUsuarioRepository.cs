using Vendex.Domain.Entities;

namespace Vendex.Domain.Interfaces;

public interface IUsuarioRepository : IRepository<Usuario>
{
    Task<Usuario?> ObterPorLoginAsync(string login);
}
