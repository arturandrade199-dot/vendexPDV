using Vendex.Domain.Entities;

namespace Vendex.Application.Services;

public interface IProdutoService
{
    Task<IReadOnlyList<Produto>> ListarAsync();
    Task<ResumoProdutos> ObterResumoAsync();
    Task<Produto> AdicionarAsync(string nome, string? descricao, string? codigoBarras, decimal precoCusto, decimal precoVenda, int estoqueInicial);
    Task AtualizarAsync(int produtoId, string nome, string? descricao, string? codigoBarras, decimal precoCusto, decimal precoVenda, int estoque);
    Task AlternarAtivoAsync(int produtoId);
}
