using Vendex.Domain.Entities;
using Vendex.Domain.Interfaces;

namespace Vendex.Application.Services;

public class ProdutoService : IProdutoService
{
    private const int EstoqueBaixoLimite = 5;

    private readonly IUnitOfWork _unitOfWork;

    public ProdutoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<IReadOnlyList<Produto>> ListarAsync() => _unitOfWork.Produtos.ObterTodosAsync();

    public async Task<ResumoProdutos> ObterResumoAsync()
    {
        var produtos = await ListarAsync();
        var ativos = produtos.Where(p => p.Ativo).ToList();

        var estoqueBaixo = ativos.Count(p => p.EstoqueAtual <= EstoqueBaixoLimite);
        var valorEmEstoque = ativos.Sum(p => p.PrecoCusto * p.EstoqueAtual);

        return new ResumoProdutos(produtos.Count, ativos.Count, estoqueBaixo, valorEmEstoque);
    }

    public async Task<Produto> AdicionarAsync(string nome, string? descricao, string? codigoBarras, decimal precoCusto, decimal precoVenda, int estoqueInicial)
    {
        var produto = new Produto
        {
            Nome = nome,
            Descricao = descricao,
            CodigoBarras = codigoBarras,
            PrecoCusto = precoCusto,
            PrecoVenda = precoVenda,
            EstoqueAtual = estoqueInicial,
            Ativo = true
        };

        await _unitOfWork.Produtos.AdicionarAsync(produto);
        await _unitOfWork.SalvarAlteracoesAsync();
        return produto;
    }

    public async Task AtualizarAsync(int produtoId, string nome, string? descricao, string? codigoBarras, decimal precoCusto, decimal precoVenda, int estoque)
    {
        var produto = await _unitOfWork.Produtos.ObterPorIdAsync(produtoId);
        if (produto is null)
            return;

        produto.Nome = nome;
        produto.Descricao = descricao;
        produto.CodigoBarras = codigoBarras;
        produto.PrecoCusto = precoCusto;
        produto.PrecoVenda = precoVenda;
        produto.EstoqueAtual = estoque;

        _unitOfWork.Produtos.Atualizar(produto);
        await _unitOfWork.SalvarAlteracoesAsync();
    }

    public async Task AlternarAtivoAsync(int produtoId)
    {
        var produto = await _unitOfWork.Produtos.ObterPorIdAsync(produtoId);
        if (produto is null)
            return;

        produto.Ativo = !produto.Ativo;
        _unitOfWork.Produtos.Atualizar(produto);
        await _unitOfWork.SalvarAlteracoesAsync();
    }
}
