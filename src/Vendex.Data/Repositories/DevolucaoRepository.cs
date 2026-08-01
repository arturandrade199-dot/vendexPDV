using Microsoft.EntityFrameworkCore;
using Vendex.Domain.Entities;
using Vendex.Domain.Interfaces;

namespace Vendex.Data.Repositories;

public class DevolucaoRepository : Repository<Devolucao>, IDevolucaoRepository
{
    public DevolucaoRepository(VendexDbContext contexto) : base(contexto)
    {
    }

    public async Task<IReadOnlyList<Devolucao>> ObterTodosComClienteAsync() =>
        await DbSet
            .Include(d => d.Cliente)
            .Include(d => d.Itens)
            .AsNoTracking()
            .OrderByDescending(d => d.DataHora)
            .ToListAsync();

    public async Task<IReadOnlyList<DevolucaoItem>> ObterItensPorPeriodoAsync(DateTime inicio, DateTime fim) =>
        await Contexto.DevolucaoItens
            .Include(i => i.Devolucao).ThenInclude(d => d.Cliente)
            .Include(i => i.Produto)
            .Include(i => i.ProdutoVariante)
            .Where(i => i.Devolucao.DataHora >= inicio && i.Devolucao.DataHora <= fim)
            .AsNoTracking()
            .ToListAsync();
}
