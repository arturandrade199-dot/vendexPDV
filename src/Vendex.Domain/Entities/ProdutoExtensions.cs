namespace Vendex.Domain.Entities;

public static class ProdutoExtensions
{
    public static decimal EstoqueTotal(this Produto produto) =>
        produto.TemGrade ? produto.Variantes.Sum(v => v.EstoqueAtual) : produto.EstoqueAtual;
}
