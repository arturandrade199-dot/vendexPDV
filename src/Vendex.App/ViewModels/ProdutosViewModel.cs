using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.App.Navigation;
using Vendex.Application.Services;
using Vendex.Domain.Entities;

namespace Vendex.App.ViewModels;

public partial class ProdutosViewModel : ObservableObject
{
    private const string NomeModulo = "Produtos";

    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IProdutoService _produtoService;
    private readonly Func<Produto?, ProdutoWindow> _produtoWindowFactory;
    private readonly SessaoUsuario _sessao;

    public ObservableCollection<ProdutoLinhaViewModel> Produtos { get; } = new();

    [ObservableProperty] private int totalProdutos;
    [ObservableProperty] private int ativos;
    [ObservableProperty] private int estoqueBaixo;
    [ObservableProperty] private string valorEmEstoqueFormatado = "R$ 0,00";

    public bool PodeCriar => _sessao.PodeCriar(NomeModulo);
    public bool PodeEditar => _sessao.PodeEditar(NomeModulo);
    public bool PodeExcluir => _sessao.PodeExcluir(NomeModulo);

    public ProdutosViewModel(IProdutoService produtoService, Func<Produto?, ProdutoWindow> produtoWindowFactory, SessaoUsuario sessao)
    {
        _produtoService = produtoService;
        _produtoWindowFactory = produtoWindowFactory;
        _sessao = sessao;
        _ = CarregarAsync();
    }

    [RelayCommand]
    private async Task AdicionarAsync()
    {
        if (!PodeCriar) return;

        var janela = _produtoWindowFactory(null);
        if (janela.ShowDialog() == true)
        {
            await CarregarAsync();
        }
    }

    [RelayCommand]
    private async Task EditarAsync(ProdutoLinhaViewModel linha)
    {
        if (!PodeEditar) return;

        var produto = await _produtoService.ListarAsync();
        var alvo = produto.FirstOrDefault(p => p.Id == linha.Id);
        if (alvo is null)
            return;

        var janela = _produtoWindowFactory(alvo);
        if (janela.ShowDialog() == true)
        {
            await CarregarAsync();
        }
    }

    [RelayCommand]
    private async Task AlternarAtivoAsync(ProdutoLinhaViewModel linha)
    {
        if (!PodeExcluir) return;

        await _produtoService.AlternarAtivoAsync(linha.Id);
        await CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        var produtos = await _produtoService.ListarAsync();
        Produtos.Clear();
        foreach (var produto in produtos.OrderBy(p => p.Nome))
            Produtos.Add(new ProdutoLinhaViewModel(produto));

        var resumo = await _produtoService.ObterResumoAsync();
        TotalProdutos = resumo.TotalProdutos;
        Ativos = resumo.Ativos;
        EstoqueBaixo = resumo.EstoqueBaixo;
        ValorEmEstoqueFormatado = resumo.ValorEmEstoque.ToString("C2", CulturaBr);
    }
}
