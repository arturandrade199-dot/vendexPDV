using Microsoft.EntityFrameworkCore;
using Vendex.Domain.Entities;
using Vendex.Domain.Enums;
using Vendex.Domain.Interfaces;

namespace Vendex.Data.Repositories;

public class CaixaRepository : Repository<Caixa>, ICaixaRepository
{
    public CaixaRepository(VendexDbContext contexto) : base(contexto)
    {
    }

    public Task<Caixa?> ObterCaixaAbertoAsync() =>
        DbSet.Include(c => c.Movimentacoes).FirstOrDefaultAsync(c => c.Status == StatusCaixa.Aberto);
}
