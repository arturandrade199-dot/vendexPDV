using Microsoft.EntityFrameworkCore;
using Vendex.Domain.Entities;
using Vendex.Domain.Interfaces;

namespace Vendex.Data.Repositories;

public class VendaRepository : Repository<Venda>, IVendaRepository
{
    public VendaRepository(VendexDbContext contexto) : base(contexto)
    {
    }

    public Task<Venda?> ObterComItensAsync(int id) =>
        DbSet.Include(v => v.Itens).ThenInclude(i => i.Produto).FirstOrDefaultAsync(v => v.Id == id);
}
