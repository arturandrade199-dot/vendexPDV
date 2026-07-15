using Vendex.Domain.Entities;

namespace Vendex.Domain.Interfaces;

public interface IContaPagarRepository : IRepository<ContaPagar>
{
    Task<IReadOnlyList<ContaPagar>> ObterTodosComFornecedorAsync();
    Task<IReadOnlyList<ContaPagar>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim);
    Task<IReadOnlyList<ContaPagarPagamento>> ObterPagamentosPorPeriodoAsync(DateTime inicio, DateTime fim);
}
