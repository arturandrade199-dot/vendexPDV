using Vendex.Domain.Entities;

namespace Vendex.Domain.Interfaces;

public interface ILoteRepository : IRepository<Lote>
{
    Task<IReadOnlyList<Lote>> ObterTodosComProdutoAsync();
    Task<Lote?> ObterComProdutoAsync(int id);
    Task<IReadOnlyList<LotePerda>> ObterPerdasPorPeriodoAsync(DateTime inicio, DateTime fim);
}
