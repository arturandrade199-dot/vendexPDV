using Vendex.Domain.Entities;

namespace Vendex.Domain.Interfaces;

public interface IDevolucaoRepository : IRepository<Devolucao>
{
    Task<IReadOnlyList<Devolucao>> ObterTodosComClienteAsync();
    Task<IReadOnlyList<DevolucaoItem>> ObterItensPorPeriodoAsync(DateTime inicio, DateTime fim);
}
