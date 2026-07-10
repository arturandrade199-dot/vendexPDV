namespace Vendex.Domain.Entities;

public class CaixaAberturaDetalhe : EntidadeBase
{
    public int CaixaId { get; set; }
    public Caixa Caixa { get; set; } = null!;

    public decimal TipoCedula { get; set; }
    public int Quantidade { get; set; }
    public decimal Subtotal { get; set; }
}

public class CaixaFechamentoDetalhe : EntidadeBase
{
    public int CaixaId { get; set; }
    public Caixa Caixa { get; set; } = null!;

    public decimal TipoCedula { get; set; }
    public int Quantidade { get; set; }
    public decimal Subtotal { get; set; }
}
