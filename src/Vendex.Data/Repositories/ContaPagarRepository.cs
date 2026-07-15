using Microsoft.EntityFrameworkCore;
using Vendex.Domain.Entities;
using Vendex.Domain.Interfaces;

namespace Vendex.Data.Repositories;

public class ContaPagarRepository : Repository<ContaPagar>, IContaPagarRepository
{
    public ContaPagarRepository(VendexDbContext contexto) : base(contexto)
    {
    }

    public async Task<IReadOnlyList<ContaPagar>> ObterTodosComFornecedorAsync() =>
        await DbSet.Include(c => c.Fornecedor).AsNoTracking().ToListAsync();
}
