using System.Globalization;
using System.Windows.Media;
using Vendex.Domain.Entities;

namespace Vendex.App.ViewModels;

/// <summary>
/// Envelope somente-leitura de um Produto para exibição na tabela — mesma ideia do
/// ContaPagarLinhaViewModel: mantém a View sem lógica de formatação/cor.
/// </summary>
public class ProdutoLinhaViewModel
{
    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");
    private const int EstoqueBaixoLimite = 5;

    public ProdutoLinhaViewModel(Produto produto)
    {
        Id = produto.Id;
        Nome = produto.Nome;
        CodigoBarras = string.IsNullOrWhiteSpace(produto.CodigoBarras) ? "—" : produto.CodigoBarras;
        PrecoCusto = produto.PrecoCusto;
        PrecoVenda = produto.PrecoVenda;
        PrecoCustoFormatado = produto.PrecoCusto.ToString("C2", CulturaBr);
        PrecoVendaFormatado = produto.PrecoVenda.ToString("C2", CulturaBr);
        Estoque = produto.EstoqueAtual;
        Descricao = produto.Descricao ?? string.Empty;
        Ativo = produto.Ativo;

        EstoqueCor = produto.EstoqueAtual <= EstoqueBaixoLimite
            ? Color.FromRgb(0xC4, 0x2B, 0x1E)
            : Color.FromRgb(0x1F, 0x24, 0x30);

        (SituacaoTexto, SituacaoFundo, SituacaoCor) = produto.Ativo
            ? ("Ativo", Color.FromRgb(0xDC, 0xF5, 0xE3), Color.FromRgb(0x1B, 0x8A, 0x4B))
            : ("Inativo", Color.FromRgb(0xF1, 0xF2, 0xF4), Color.FromRgb(0x6B, 0x72, 0x80));
    }

    public int Id { get; }
    public string Nome { get; }
    public string CodigoBarras { get; }
    public string Descricao { get; }
    public decimal PrecoCusto { get; }
    public decimal PrecoVenda { get; }
    public string PrecoCustoFormatado { get; }
    public string PrecoVendaFormatado { get; }
    public int Estoque { get; }
    public Color EstoqueCor { get; }
    public bool Ativo { get; }
    public string SituacaoTexto { get; }
    public Color SituacaoFundo { get; }
    public Color SituacaoCor { get; }
    public string RotuloAlternarAtivo => Ativo ? "Desativar" : "Ativar";
}
