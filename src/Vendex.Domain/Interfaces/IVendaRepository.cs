using Vendex.Domain.Entities;

namespace Vendex.Domain.Interfaces;

public interface IVendaRepository : IRepository<Venda>
{
    Task<Venda?> ObterComItensAsync(int id);
}
