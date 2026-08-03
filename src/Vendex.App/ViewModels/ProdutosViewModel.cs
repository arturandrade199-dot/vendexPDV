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
    private List<Produto> _todosProdutos = new();

    public ObservableCollection<ProdutoLinhaViewModel> Produtos { get; } = new();
    public ObservableCollection<string> SituacoesDisponiveis { get; } = new() { "Todos", "Ativos", "Inativos" };

    [ObservableProperty] private int totalProdutos;
    [ObservableProperty] private int ativos;
    [ObservableProperty] private int estoqueBaixo;
    [ObservableProperty] private string valorEmEstoqueFormatado = "R$ 0,00";
    [ObservableProperty] private ProdutoLinhaViewModel? itemSelecionado;
    [ObservableProperty] private string termoBusca = string.Empty;
    [ObservableProperty] private string situacaoSelecionada = "Todos";

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
    private async Task EditarAsync(ProdutoLinhaViewModel? linha)
    {
        // linha vem null quando disparado pelo atalho de teclado (Enter) sem nenhuma linha
        // selecionada no grid — clique no botão "Editar" sempre manda uma linha concreta.
        if (!PodeEditar || linha is null) return;

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
    private async Task AlternarAtivoAsync(ProdutoLinhaViewModel? linha)
    {
        if (!PodeExcluir || linha is null) return;

        await _produtoService.AlternarAtivoAsync(linha.Id);
        await CarregarAsync();
    }

    partial void OnTermoBuscaChanged(string value) => AplicarFiltro();
    partial void OnSituacaoSelecionadaChanged(string value) => AplicarFiltro();

    private async Task CarregarAsync()
    {
        _todosProdutos = (await _produtoService.ListarAsync()).OrderBy(p => p.Nome).ToList();

        var resumo = await _produtoService.ObterResumoAsync();
        TotalProdutos = resumo.TotalProdutos;
        Ativos = resumo.Ativos;
        EstoqueBaixo = resumo.EstoqueBaixo;
        ValorEmEstoqueFormatado = resumo.ValorEmEstoque.ToString("C2", CulturaBr);

        AplicarFiltro();
    }

    private void AplicarFiltro()
    {
        var termo = TermoBusca.Trim();
        IEnumerable<Produto> filtrados = _todosProdutos;

        if (!string.IsNullOrEmpty(termo))
            filtrados = filtrados.Where(p =>
                p.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                (p.CodigoBarras?.Contains(termo, StringComparison.OrdinalIgnoreCase) ?? false));

        filtrados = SituacaoSelecionada switch
        {
            "Ativos" => filtrados.Where(p => p.Ativo),
            "Inativos" => filtrados.Where(p => !p.Ativo),
            _ => filtrados
        };

        Produtos.Clear();
        foreach (var produto in filtrados)
            Produtos.Add(new ProdutoLinhaViewModel(produto));
    }
}
