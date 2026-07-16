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

    public async Task<IReadOnlyList<Venda>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim) =>
        await DbSet
            .Include(v => v.Itens).ThenInclude(i => i.Produto)
            .Include(v => v.Pagamentos)
            .Include(v => v.Usuario)
            .Include(v => v.Cliente)
            .Where(v => !v.Cancelada && v.DataHora >= inicio && v.DataHora <= fim)
            .ToListAsync();
}
