namespace Vendex.Application.Services;

public record ItemCarrinho(int ProdutoId, int? ProdutoVarianteId, decimal Quantidade, decimal PrecoUnitario, decimal PrecoCustoUnitario);
