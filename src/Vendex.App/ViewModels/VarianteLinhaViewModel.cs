using CommunityToolkit.Mvvm.ComponentModel;

namespace Vendex.App.ViewModels;

/// <summary>Linha editável da grade de variantes no cadastro de produto — Id nulo
/// significa variante nova (ainda não persistida).</summary>
public partial class VarianteLinhaViewModel : ObservableObject
{
    public int? Id { get; }

    [ObservableProperty] private string nome = string.Empty;
    [ObservableProperty] private string codigoBarras = string.Empty;
    [ObservableProperty] private string estoqueTexto = "0";

    public VarianteLinhaViewModel()
    {
    }

    public VarianteLinhaViewModel(int id, string nome, string? codigoBarras, decimal estoqueAtual)
    {
        Id = id;
        Nome = nome;
        CodigoBarras = codigoBarras ?? string.Empty;
        EstoqueTexto = estoqueAtual.ToString("0.###");
    }
}
