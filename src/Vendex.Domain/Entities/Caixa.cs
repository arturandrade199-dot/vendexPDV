using Vendex.Domain.Enums;

namespace Vendex.Domain.Entities;

public class Caixa : EntidadeBase
{
    public DateTime DataAbertura { get; set; } = DateTime.Now;
    public int UsuarioAberturaId { get; set; }
    public Usuario UsuarioAbertura { get; set; } = null!;
    public decimal ValorAberturaTotal { get; set; }

    public DateTime? DataFechamento { get; set; }
    public int? UsuarioFechamentoId { get; set; }
    public Usuario? UsuarioFechamento { get; set; }
    public decimal? ValorFechamentoTotal { get; set; }

    public decimal? FaturamentoTotal { get; set; }
    public decimal? CustoTotal { get; set; }
    public decimal? LucroTotal { get; set; }

    public StatusCaixa Status { get; set; } = StatusCaixa.Aberto;

    public ICollection<CaixaAberturaDetalhe> AberturaDetalhes { get; set; } = new List<CaixaAberturaDetalhe>();
    public ICollection<CaixaFechamentoDetalhe> FechamentoDetalhes { get; set; } = new List<CaixaFechamentoDetalhe>();
    public ICollection<CaixaMovimentacao> Movimentacoes { get; set; } = new List<CaixaMovimentacao>();
}
