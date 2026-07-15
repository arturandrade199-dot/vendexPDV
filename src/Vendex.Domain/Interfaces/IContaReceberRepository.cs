using Vendex.Domain.Entities;

namespace Vendex.Domain.Interfaces;

public interface IContaReceberRepository : IRepository<ContaReceber>
{
    Task<IReadOnlyList<ContaReceber>> ObterTodosComClienteAsync();
    Task<IReadOnlyList<ContaReceber>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim);
    Task<IReadOnlyList<ContaReceberPagamento>> ObterPagamentosPorPeriodoAsync(DateTime inicio, DateTime fim);
}
