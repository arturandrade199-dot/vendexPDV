using Vendex.Domain.Enums;

namespace Vendex.Domain.Entities;

/// <summary>
/// Uma linha de pagamento aplicada a uma venda. Uma Venda pode ter várias (pagamento
/// misto — parte no cartão, parte em dinheiro, etc). Para linhas em Dinheiro, Valor é o
/// valor efetivamente aplicado à venda (não o valor entregue pelo cliente) — o troco é
/// calculado e exibido na hora, mas não é uma linha de pagamento nem entra na receita.
/// </summary>
public class VendaPagamento : EntidadeBase
{
    public int VendaId { get; set; }
    public Venda Venda { get; set; } = null!;

    public FormaPagamento FormaPagamento { get; set; }
    public decimal Valor { get; set; }
}
