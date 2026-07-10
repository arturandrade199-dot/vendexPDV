using Microsoft.EntityFrameworkCore;
using Vendex.Domain.Entities;
using Vendex.Domain.Interfaces;

namespace Vendex.Data.Repositories;

public class UsuarioRepository : Repository<Usuario>, IUsuarioRepository
{
    public UsuarioRepository(VendexDbContext contexto) : base(contexto)
    {
    }

    public Task<Usuario?> ObterPorLoginAsync(string login) =>
        DbSet.FirstOrDefaultAsync(u => u.Login == login);
}
