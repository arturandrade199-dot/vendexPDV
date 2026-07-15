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

    public async Task<IReadOnlyList<ContaPagar>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim) =>
        await DbSet
            .Include(c => c.Fornecedor)
            .Where(c => c.DataVencimento >= inicio && c.DataVencimento <= fim)
            .AsNoTracking()
            .ToListAsync();

    public async Task<IReadOnlyList<ContaPagarPagamento>> ObterPagamentosPorPeriodoAsync(DateTime inicio, DateTime fim) =>
        await Contexto.ContasPagarPagamentos
            .Include(p => p.ContaPagar).ThenInclude(c => c.Fornecedor)
            .Where(p => p.DataPagamento >= inicio && p.DataPagamento <= fim)
            .AsNoTracking()
            .ToListAsync();
}
