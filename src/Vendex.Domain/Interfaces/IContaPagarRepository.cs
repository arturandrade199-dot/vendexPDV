using Vendex.Domain.Entities;

namespace Vendex.Domain.Interfaces;

public interface IContaPagarRepository : IRepository<ContaPagar>
{
    Task<IReadOnlyList<ContaPagar>> ObterTodosComFornecedorAsync();
}
