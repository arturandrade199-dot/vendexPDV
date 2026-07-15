using Vendex.Domain.Entities;

namespace Vendex.Domain.Interfaces;

public interface IContaReceberRepository : IRepository<ContaReceber>
{
    Task<IReadOnlyList<ContaReceber>> ObterTodosComClienteAsync();
}
