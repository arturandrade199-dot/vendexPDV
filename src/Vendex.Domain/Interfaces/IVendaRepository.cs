using Vendex.Domain.Entities;

namespace Vendex.Domain.Interfaces;

public interface IVendaRepository : IRepository<Venda>
{
    Task<Venda?> ObterComItensAsync(int id);
    Task<IReadOnlyList<Venda>> ObterPorPeriodoAsync(DateTime inicio, DateTime fim);

    /// <summary>Últimas vendas do cliente, com itens/produtos já carregados — usado pela tela
    /// de devolução pra deixar o operador escolher uma venda e carregar os itens dela.</summary>
    Task<IReadOnlyList<Venda>> ObterPorClienteAsync(int clienteId);
}
