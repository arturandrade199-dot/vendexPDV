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

    public async Task<IReadOnlyList<ContaReceber>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim) =>
        await DbSet
            .Include(c => c.Cliente)
            .Where(c => c.DataVencimento >= inicio && c.DataVencimento <= fim)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IReadOnlyList<ContaReceberPagamento>> ObterPagamentosPorPeriodoAsync(DateTime inicio, DateTime fim) =>
        await Contexto.ContasReceberPagamentos
            .Include(p => p.ContaReceber).ThenInclude(c => c.Cliente)
            .Where(p => p.DataPagamento >= inicio && p.DataPagamento <= fim)
            .AsNoTracking()
            .ToListAsync();
}
