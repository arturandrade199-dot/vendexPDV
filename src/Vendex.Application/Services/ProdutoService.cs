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

        var estoqueBaixo = ativos.Count(p => p.EstoqueTotal() <= EstoqueBaixoLimite);
        var valorEmEstoque = ativos.Sum(p => p.PrecoCusto * p.EstoqueTotal());

        return new ResumoProdutos(produtos.Count, ativos.Count, estoqueBaixo, valorEmEstoque);
    }

    public async Task<Produto> AdicionarAsync(ProdutoInput input)
    {
        var produto = new Produto
        {
            Nome = input.Nome,
            CodigoBarras = input.TemGrade ? null : input.CodigoBarras,
            PrecoCusto = input.PrecoCusto,
            PrecoVenda = input.PrecoVenda,
            EstoqueAtual = input.TemGrade ? 0 : input.EstoqueAtual,
            UnidadeMedida = input.UnidadeMedida,
            TemGrade = input.TemGrade,
            Ativo = true
        };

        if (input.TemGrade)
        {
            foreach (var vi in input.Variantes)
            {
                produto.Variantes.Add(new ProdutoVariante
                {
                    Nome = vi.Nome,
                    CodigoBarras = vi.CodigoBarras,
                    EstoqueAtual = vi.EstoqueAtual
                });
            }
        }

        await _unitOfWork.Produtos.AdicionarAsync(produto);
        await _unitOfWork.SalvarAlteracoesAsync();
        return produto;
    }

    public async Task AtualizarAsync(int produtoId, ProdutoInput input)
    {
        var produto = await _unitOfWork.Produtos.ObterPorIdAsync(produtoId);
        if (produto is null)
            return;

        produto.Nome = input.Nome;
        produto.CodigoBarras = input.TemGrade ? null : input.CodigoBarras;
        produto.PrecoCusto = input.PrecoCusto;
        produto.PrecoVenda = input.PrecoVenda;
        produto.UnidadeMedida = input.UnidadeMedida;
        produto.TemGrade = input.TemGrade;
        produto.EstoqueAtual = input.TemGrade ? 0 : input.EstoqueAtual;

        SincronizarVariantes(produto, input.TemGrade ? input.Variantes : Array.Empty<VarianteInput>());

        _unitOfWork.Produtos.Atualizar(produto);
        await _unitOfWork.SalvarAlteracoesAsync();
    }

    private static void SincronizarVariantes(Produto produto, IReadOnlyList<VarianteInput> variantesInput)
    {
        var idsRecebidos = variantesInput.Where(v => v.Id is not null).Select(v => v.Id!.Value).ToHashSet();

        // Remove as que sumiram da lista — EF Core apaga o órfão ao salvar, pois a FK é obrigatória.
        foreach (var existente in produto.Variantes.Where(v => !idsRecebidos.Contains(v.Id)).ToList())
            produto.Variantes.Remove(existente);

        foreach (var vi in variantesInput)
        {
            if (vi.Id is int id)
            {
                var alvo = produto.Variantes.First(v => v.Id == id);
                alvo.Nome = vi.Nome;
                alvo.CodigoBarras = vi.CodigoBarras;
                alvo.EstoqueAtual = vi.EstoqueAtual;
            }
            else
            {
                produto.Variantes.Add(new ProdutoVariante
                {
                    Nome = vi.Nome,
                    CodigoBarras = vi.CodigoBarras,
                    EstoqueAtual = vi.EstoqueAtual
                });
            }
        }
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
