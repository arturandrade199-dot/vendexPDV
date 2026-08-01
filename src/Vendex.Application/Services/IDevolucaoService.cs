using Vendex.Domain.Entities;

namespace Vendex.Application.Services;

public interface IDevolucaoService
{
    Task<IReadOnlyList<Venda>> ListarVendasPorClienteAsync(int clienteId);

    /// <summary>Registra a devolução: dá entrada no estoque de cada item e, se
    /// <paramref name="estornarCaixa"/> for true, lança uma saída de caixa (Sangria) no valor
    /// total. `vendaId` é opcional — nulo para devolução avulsa, sem vínculo com uma venda
    /// específica do histórico.</summary>
    Task<Devolucao> RegistrarAsync(
        int clienteId, int? vendaId, int usuarioId, string? motivo,
        IReadOnlyList<ItemDevolucao> itens, bool estornarCaixa);
}
