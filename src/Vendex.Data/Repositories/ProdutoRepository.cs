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
}
