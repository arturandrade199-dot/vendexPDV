using Vendex.Domain.Entities;

namespace Vendex.Application.Services;

public interface IVendaService
{
    Task<IReadOnlyList<Produto>> BuscarProdutosAsync(string termo);

    /// <summary>
    /// Finaliza a venda com uma ou mais formas de pagamento (pagamento misto). A soma de
    /// `pagamentos` deve ser igual ao total dos itens. Se houver uma linha Fiado, `clienteId`
    /// é obrigatório e um lançamento em ContaReceber é criado para o valor daquela linha.
    /// </summary>
    Task<Venda> FinalizarVendaAsync(
        IReadOnlyList<ItemCarrinho> itens,
        IReadOnlyList<PagamentoAplicado> pagamentos,
        int usuarioId,
        int? clienteId = null,
        DateTime? vencimentoFiado = null);

    /// <summary>Vendas não canceladas no período, com itens/produtos e pagamentos já
    /// carregados — usado pela tela de Vendas para listar e reimprimir cupons.</summary>
    Task<IReadOnlyList<Venda>> ListarPorPeriodoAsync(DateTime inicio, DateTime fim);
}
