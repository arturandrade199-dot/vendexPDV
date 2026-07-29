using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Vendex.Application.Services;

namespace Vendex.App.ViewModels;

public partial class ItemCarrinhoViewModel : ObservableObject
{
    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubtotalFormatado))]
    [NotifyPropertyChangedFor(nameof(QuantidadeFormatada))]
    private decimal quantidade;

    /// <summary>Item cancelado durante a venda em andamento — permanece visível no grid
    /// (riscado), mas fica fora do total e não entra no cupom nem na venda finalizada.</summary>
    [ObservableProperty] private bool removido;

    public ItemCarrinhoViewModel(ResultadoBuscaProduto resultado, decimal quantidadeInicial)
        : this(resultado.NomeExibicao, resultado.ProdutoId, resultado.VarianteId, resultado.PrecoVenda, resultado.PrecoCusto, quantidadeInicial)
    {
    }

    /// <summary>Reconstrói o item a partir de valores históricos (ex: um VendaItem já
    /// finalizado) em vez do Produto atual — sem isso, reimprimir o cupom de uma venda
    /// antiga mostraria o preço de HOJE do produto, não o preço cobrado na hora da venda.</summary>
    public ItemCarrinhoViewModel(string nome, int produtoId, int? produtoVarianteId, decimal precoUnitario, decimal precoCusto, decimal quantidadeInicial)
    {
        ProdutoId = produtoId;
        ProdutoVarianteId = produtoVarianteId;
        Nome = nome;
        PrecoUnitario = precoUnitario;
        PrecoCusto = precoCusto;
        Quantidade = quantidadeInicial;
    }

    public int ProdutoId { get; }
    public int? ProdutoVarianteId { get; }
    public string Nome { get; }
    public decimal PrecoUnitario { get; }
    public decimal PrecoCusto { get; }
    public string PrecoUnitarioFormatado => PrecoUnitario.ToString("C2", CulturaBr);
    public string QuantidadeFormatada => Quantidade.ToString("0.###", CulturaBr);
    public decimal Subtotal => PrecoUnitario * Quantidade;
    public string SubtotalFormatado => Subtotal.ToString("C2", CulturaBr);
}
