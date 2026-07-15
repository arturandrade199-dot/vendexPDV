using Microsoft.EntityFrameworkCore;
using Vendex.Domain.Entities;
using Vendex.Domain.Interfaces;

namespace Vendex.Data.Repositories;

public class ContaReceberRepository : Repository<ContaReceber>, IContaReceberRepository
{
    public ContaReceberRepository(VendexDbContext contexto) : base(contexto)
    {
    }

    public async Task<IReadOnlyList<ContaReceber>> ObterTodosComClienteAsync() =>
        await DbSet.Include(c => c.Cliente).AsNoTracking().ToListAsync();
}
