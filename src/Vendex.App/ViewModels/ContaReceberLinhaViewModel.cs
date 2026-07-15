using System.Globalization;
using System.Windows.Media;
using Vendex.Domain.Entities;
using Vendex.Domain.Enums;

namespace Vendex.App.ViewModels;

/// <summary>
/// Envelope somente-leitura de uma ContaReceber para exibição na tabela — mesma ideia do
/// ContaPagarLinhaViewModel: mantém a View sem lógica de formatação/cor.
/// </summary>
public class ContaReceberLinhaViewModel
{
    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    public ContaReceberLinhaViewModel(ContaReceber conta)
    {
        Id = conta.Id;
        Vencimento = conta.DataVencimento.ToString("dd/MM/yyyy", CulturaBr);
        Descricao = conta.Descricao;
        ClienteNome = conta.Cliente?.Nome ?? "—";
        TotalFormatado = conta.ValorTotal.ToString("C2", CulturaBr);
        PodeMarcarComoRecebido = conta.Status != StatusContaFinanceira.Pago;

        (SituacaoTexto, SituacaoFundo, SituacaoCor) = conta.Status switch
        {
            StatusContaFinanceira.Pago => ("Recebido", Color.FromRgb(0xDC, 0xF5, 0xE3), Color.FromRgb(0x1B, 0x8A, 0x4B)),
            StatusContaFinanceira.Atrasado => ("Atrasado", Color.FromRgb(0xFD, 0xE4, 0xE1), Color.FromRgb(0xC4, 0x2B, 0x1E)),
            StatusContaFinanceira.Parcial => ("Parcial", Color.FromRgb(0xE3, 0xEC, 0xFD), Color.FromRgb(0x25, 0x5B, 0xC4)),
            _ => ("Em aberto", Color.FromRgb(0xFD, 0xF3, 0xD9), Color.FromRgb(0xB8, 0x7A, 0x0A))
        };
    }

    public int Id { get; }
    public string Vencimento { get; }
    public string Descricao { get; }
    public string ClienteNome { get; }
    public string TotalFormatado { get; }
    public string SituacaoTexto { get; }
    public Color SituacaoFundo { get; }
    public Color SituacaoCor { get; }
    public bool PodeMarcarComoRecebido { get; }
}
