using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using Vendex.Domain.Entities;

namespace Vendex.App.ViewModels;

public partial class ItemCarrinhoViewModel : ObservableObject
{
    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubtotalFormatado))]
    private int quantidade;

    public ItemCarrinhoViewModel(Produto produto, int quantidadeInicial)
        : this(produto.Nome, produto.Id, produto.PrecoVenda, produto.PrecoCusto, quantidadeInicial)
    {
    }

    /// <summary>Reconstrói o item a partir de valores históricos (ex: um VendaItem já
    /// finalizado) em vez do Produto atual — sem isso, reimprimir o cupom de uma venda
    /// antiga mostraria o preço de HOJE do produto, não o preço cobrado na hora da venda.</summary>
    public ItemCarrinhoViewModel(string nome, int produtoId, decimal precoUnitario, decimal precoCusto, int quantidadeInicial)
    {
        ProdutoId = produtoId;
        Nome = nome;
        PrecoUnitario = precoUnitario;
        PrecoCusto = precoCusto;
        Quantidade = quantidadeInicial;
    }

    public int ProdutoId { get; }
    public string Nome { get; }
    public decimal PrecoUnitario { get; }
    public decimal PrecoCusto { get; }
    public string PrecoUnitarioFormatado => PrecoUnitario.ToString("C2", CulturaBr);
    public decimal Subtotal => PrecoUnitario * Quantidade;
    public string SubtotalFormatado => Subtotal.ToString("C2", CulturaBr);
}
