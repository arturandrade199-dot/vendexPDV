using System.Globalization;
using Vendex.Domain.Enums;

namespace Vendex.App.ViewModels;

public class LinhaPagamentoViewModel
{
    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    public LinhaPagamentoViewModel(FormaPagamento forma, string formaTexto, decimal valor, decimal? troco, string? clienteNome)
    {
        Forma = forma;
        FormaTexto = formaTexto;
        Valor = valor;
        ValorFormatado = valor.ToString("C2", CulturaBr);
        Troco = troco;
        TrocoFormatado = troco is > 0 ? troco.Value.ToString("C2", CulturaBr) : null;
        ClienteNome = clienteNome;
    }

    public FormaPagamento Forma { get; }
    public string FormaTexto { get; }
    public decimal Valor { get; }
    public string ValorFormatado { get; }
    public decimal? Troco { get; }
    public string? TrocoFormatado { get; }
    public string? ClienteNome { get; }
}
