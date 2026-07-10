using Microsoft.EntityFrameworkCore;
using Vendex.Domain.Entities;
using Vendex.Domain.Interfaces;

namespace Vendex.Data.Repositories;

public class Repository<T> : IRepository<T> where T : EntidadeBase
{
    protected readonly VendexDbContext Contexto;
    protected readonly DbSet<T> DbSet;

    public Repository(VendexDbContext contexto)
    {
        Contexto = contexto;
        DbSet = contexto.Set<T>();
    }

    public Task<T?> ObterPorIdAsync(int id) => DbSet.FirstOrDefaultAsync(e => e.Id == id);

    public async Task<IReadOnlyList<T>> ObterTodosAsync() => await DbSet.AsNoTracking().ToListAsync();

    public async Task AdicionarAsync(T entidade) => await DbSet.AddAsync(entidade);

    public void Atualizar(T entidade) => DbSet.Update(entidade);

    public void Remover(T entidade) => DbSet.Remove(entidade);
}
