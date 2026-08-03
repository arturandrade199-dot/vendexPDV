using System.Globalization;
using System.Windows.Media;
using Vendex.Domain.Entities;
using Vendex.Domain.Enums;

namespace Vendex.App.ViewModels;

/// <summary>
/// Envelope somente-leitura de um Lote para exibição na tabela — mesma ideia do
/// ContaPagarLinhaViewModel: mantém a View sem lógica de formatação/cor.
/// </summary>
public class LoteLinhaViewModel
{
    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    public LoteLinhaViewModel(Lote lote, int diasAlertaVencimento = 7)
    {
        Id = lote.Id;
        ProdutoNome = lote.ProdutoVariante is null ? lote.Produto.Nome : $"{lote.Produto.Nome} — {lote.ProdutoVariante.Nome}";
        ValidadeFormatada = lote.DataValidade.ToString("dd/MM/yyyy", CulturaBr);
        QuantidadeInicialFormatada = lote.QuantidadeInicial.ToString("0.###", CulturaBr);
        QuantidadeAtualFormatada = lote.QuantidadeAtual.ToString("0.###", CulturaBr);
        PodeRegistrarPerda = lote.QuantidadeAtual > 0;

        (SituacaoTexto, SituacaoFundo, SituacaoCor) = lote.SituacaoEfetiva(diasAlertaVencimento) switch
        {
            SituacaoLote.Vencido => ("Vencido", Color.FromRgb(0xFD, 0xE4, 0xE1), Color.FromRgb(0xC4, 0x2B, 0x1E)),
            SituacaoLote.Vencendo => ("Vencendo", Color.FromRgb(0xFD, 0xF3, 0xD9), Color.FromRgb(0xB8, 0x7A, 0x0A)),
            SituacaoLote.Esgotado => ("Esgotado", Color.FromRgb(0xF1, 0xF2, 0xF4), Color.FromRgb(0x6B, 0x72, 0x80)),
            _ => ("Válido", Color.FromRgb(0xDC, 0xF5, 0xE3), Color.FromRgb(0x1B, 0x8A, 0x4B))
        };
    }

    public int Id { get; }
    public string ProdutoNome { get; }
    public string ValidadeFormatada { get; }
    public string QuantidadeInicialFormatada { get; }
    public string QuantidadeAtualFormatada { get; }
    public bool PodeRegistrarPerda { get; }
    public string SituacaoTexto { get; }
    public Color SituacaoFundo { get; }
    public Color SituacaoCor { get; }
}
