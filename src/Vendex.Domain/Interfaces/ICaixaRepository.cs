using Vendex.Domain.Entities;

namespace Vendex.Domain.Interfaces;

public interface ICaixaRepository : IRepository<Caixa>
{
    Task<Caixa?> ObterCaixaAbertoAsync();
}
