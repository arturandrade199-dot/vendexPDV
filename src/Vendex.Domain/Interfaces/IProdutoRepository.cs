using Vendex.Domain.Entities;

namespace Vendex.Domain.Interfaces;

public interface IProdutoRepository : IRepository<Produto>
{
    Task<Produto?> ObterPorCodigoBarrasAsync(string codigoBarras);
}
