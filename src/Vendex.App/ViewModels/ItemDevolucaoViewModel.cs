using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Vendex.App.ViewModels;

public partial class ItemDevolucaoViewModel : ObservableObject
{
    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    public ItemDevolucaoViewModel(int produtoId, int? produtoVarianteId, string nome, decimal precoUnitario, decimal quantidadeInicial)
    {
        ProdutoId = produtoId;
        ProdutoVarianteId = produtoVarianteId;
        Nome = nome;
        PrecoUnitario = precoUnitario;
        Quantidade = quantidadeInicial;
    }

    public int ProdutoId { get; }
    public int? ProdutoVarianteId { get; }
    public string Nome { get; }
    public decimal PrecoUnitario { get; }
    public string PrecoUnitarioFormatado => PrecoUnitario.ToString("C2", CulturaBr);

    // Editável na grade — devolução parcial é o caso comum (cliente comprou 3, devolveu 1),
    // e forçar remover-e-readicionar só pra mudar a quantidade seria pior UX que um TextBox.
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(Subtotal))]
    [NotifyPropertyChangedFor(nameof(SubtotalFormatado))]
    private decimal quantidade;

    public decimal Subtotal => PrecoUnitario * Quantidade;
    public string SubtotalFormatado => Subtotal.ToString("C2", CulturaBr);
}
