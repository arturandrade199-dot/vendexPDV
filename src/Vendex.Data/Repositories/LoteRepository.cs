using Microsoft.EntityFrameworkCore;
using Vendex.Domain.Entities;
using Vendex.Domain.Interfaces;

namespace Vendex.Data.Repositories;

public class LoteRepository : Repository<Lote>, ILoteRepository
{
    public LoteRepository(VendexDbContext contexto) : base(contexto)
    {
    }

    public async Task<IReadOnlyList<Lote>> ObterTodosComProdutoAsync() =>
        await DbSet
            .Include(l => l.Produto)
            .Include(l => l.ProdutoVariante)
            .AsNoTracking()
            .OrderBy(l => l.DataValidade)
            .ToListAsync();

    public Task<Lote?> ObterComProdutoAsync(int id) =>
        DbSet
            .Include(l => l.Produto)
            .Include(l => l.ProdutoVariante)
            .Include(l => l.Perdas)
            .FirstOrDefaultAsync(l => l.Id == id);

    public async Task<IReadOnlyList<LotePerda>> ObterPerdasPorPeriodoAsync(DateTime inicio, DateTime fim) =>
        await Contexto.LotePerdas
            .Include(p => p.Lote).ThenInclude(l => l.Produto)
            .Include(p => p.Lote).ThenInclude(l => l.ProdutoVariante)
            .Where(p => p.DataHora >= inicio && p.DataHora <= fim)
            .AsNoTracking()
            .ToListAsync();
}
