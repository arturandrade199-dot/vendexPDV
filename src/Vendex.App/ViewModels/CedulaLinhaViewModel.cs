using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;

namespace Vendex.App.ViewModels;

public partial class CedulaLinhaViewModel : ObservableObject
{
    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    public CedulaLinhaViewModel(decimal valor)
    {
        Valor = valor;
        ValorFormatado = valor.ToString("C2", CulturaBr);
    }

    public decimal Valor { get; }
    public string ValorFormatado { get; }

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(SubtotalFormatado))]
    private int quantidade;

    public decimal Subtotal => Valor * Quantidade;
    public string SubtotalFormatado => Subtotal.ToString("C2", CulturaBr);
}
