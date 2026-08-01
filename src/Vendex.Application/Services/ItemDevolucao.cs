namespace Vendex.Application.Services;

/// <summary>Uma linha a devolver — igual em forma a ItemCarrinho, mas separado porque
/// representa a operação inversa (volta pro estoque) em vez de uma venda.</summary>
public record ItemDevolucao(int ProdutoId, int? ProdutoVarianteId, decimal Quantidade, decimal PrecoUnitario);
