using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.Application.Services;

namespace Vendex.App.ViewModels;

/// <summary>Diálogo de busca de produto por nome/código de barras — mesma ideia do
/// SelecionarClienteWindowViewModel, mas a busca é ao vivo contra o serviço (BuscarProdutosAsync
/// já resolve variantes de grade) em vez de filtrar uma lista fixa carregada de antemão.</summary>
public partial class SelecionarProdutoWindowViewModel : ObservableObject
{
    private readonly IVendaService _vendaService;

    public ObservableCollection<ResultadoBuscaProduto> Resultados { get; } = new();

    [ObservableProperty] private string termoBusca = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PodeConfirmar))]
    private ResultadoBuscaProduto? produtoSelecionado;

    public bool PodeConfirmar => ProdutoSelecionado is not null;

    public event Action? Confirmado;

    public SelecionarProdutoWindowViewModel(IVendaService vendaService)
    {
        _vendaService = vendaService;
        _ = BuscarAsync(TermoBusca);
    }

    partial void OnTermoBuscaChanged(string value) => _ = BuscarAsync(value);

    // Termo vazio também é uma busca válida aqui — ao contrário do type-ahead inline do
    // carrinho, este diálogo é uma listagem completa: abrir sem digitar nada já deve mostrar
    // todo o catálogo ativo (BuscarProdutosAsync("").Contains sempre bate com string vazia).
    private async Task BuscarAsync(string termo)
    {
        var resultados = await _vendaService.BuscarProdutosAsync(termo.Trim());
        Resultados.Clear();
        foreach (var resultado in resultados)
            Resultados.Add(resultado);
    }

    [RelayCommand]
    private void Confirmar()
    {
        if (ProdutoSelecionado is not null)
            Confirmado?.Invoke();
    }
}
