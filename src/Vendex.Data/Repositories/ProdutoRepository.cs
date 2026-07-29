using Microsoft.EntityFrameworkCore;
using Vendex.Domain.Entities;
using Vendex.Domain.Interfaces;

namespace Vendex.Data.Repositories;

public class ProdutoRepository : Repository<Produto>, IProdutoRepository
{
    public ProdutoRepository(VendexDbContext contexto) : base(contexto)
    {
    }

    public Task<Produto?> ObterPorCodigoBarrasAsync(string codigoBarras) =>
        DbSet.FirstOrDefaultAsync(p => p.CodigoBarras == codigoBarras);

    public new Task<Produto?> ObterPorIdAsync(int id) =>
        DbSet.Include(p => p.Variantes).FirstOrDefaultAsync(p => p.Id == id);

    public new async Task<IReadOnlyList<Produto>> ObterTodosAsync() =>
        await DbSet.Include(p => p.Variantes).AsNoTracking().ToListAsync();
}
