namespace Vendex.Application.Services;

public record ResultadoBuscaProduto(
    int ProdutoId,
    string ProdutoNome,
    int? VarianteId,
    string? VarianteNome,
    string? CodigoBarras,
    decimal PrecoVenda,
    decimal PrecoCusto,
    decimal EstoqueDisponivel)
{
    public string NomeExibicao => VarianteNome is null ? ProdutoNome : $"{ProdutoNome} — {VarianteNome}";
}
