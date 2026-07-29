using Vendex.Domain.Entities;

namespace Vendex.Application.Services;

public interface IProdutoService
{
    Task<IReadOnlyList<Produto>> ListarAsync();
    Task<ResumoProdutos> ObterResumoAsync();
    Task<Produto> AdicionarAsync(ProdutoInput input);
    Task AtualizarAsync(int produtoId, ProdutoInput input);
    Task AlternarAtivoAsync(int produtoId);
}
