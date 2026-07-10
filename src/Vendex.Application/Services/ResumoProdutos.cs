namespace Vendex.Application.Services;

public record ResumoProdutos(
    int TotalProdutos,
    int Ativos,
    int EstoqueBaixo,
    decimal ValorEmEstoque);
