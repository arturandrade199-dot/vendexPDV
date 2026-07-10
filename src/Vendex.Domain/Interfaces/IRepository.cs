using Vendex.Domain.Entities;

namespace Vendex.Domain.Interfaces;

public interface IRepository<T> where T : EntidadeBase
{
    Task<T?> ObterPorIdAsync(int id);
    Task<IReadOnlyList<T>> ObterTodosAsync();
    Task AdicionarAsync(T entidade);
    void Atualizar(T entidade);
    void Remover(T entidade);
}
