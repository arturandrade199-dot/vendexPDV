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
    {
        ProdutoId = produto.Id;
        Nome = produto.Nome;
        PrecoUnitario = produto.PrecoVenda;
        PrecoCusto = produto.PrecoCusto;
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
