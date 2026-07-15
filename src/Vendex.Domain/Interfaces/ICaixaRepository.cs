using Vendex.Domain.Entities;

namespace Vendex.Domain.Interfaces;

public interface ICaixaRepository : IRepository<Caixa>
{
    Task<Caixa?> ObterCaixaAbertoAsync();
    Task<IReadOnlyList<Caixa>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim);
}
