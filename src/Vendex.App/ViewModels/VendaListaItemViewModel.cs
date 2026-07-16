using System.Globalization;
using Vendex.Domain.Entities;
using Vendex.Domain.Enums;

namespace Vendex.App.ViewModels;

public class VendaListaItemViewModel
{
    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    public VendaListaItemViewModel(Venda venda)
    {
        Venda = venda;
        NumeroFormatado = $"#{venda.Id}";
        DataHoraFormatada = venda.DataHora.ToString("dd/MM/yyyy HH:mm", CulturaBr);
        ClienteTexto = venda.Cliente?.Nome ?? "—";
        TotalFormatado = venda.ValorTotal.ToString("C2", CulturaBr);
        FormaPagamentoResumo = string.Join(", ", venda.Pagamentos.Select(p => p.FormaPagamento.ParaTexto()).Distinct());
        UsuarioNome = venda.Usuario?.Nome ?? "—";
    }

    public Venda Venda { get; }
    public string NumeroFormatado { get; }
    public string DataHoraFormatada { get; }
    public string ClienteTexto { get; }
    public string TotalFormatado { get; }
    public string FormaPagamentoResumo { get; }
    public string UsuarioNome { get; }
}
