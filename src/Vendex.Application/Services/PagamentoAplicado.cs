using Vendex.Domain.Enums;

namespace Vendex.Application.Services;

/// <summary>Uma linha de pagamento informada na finalização da venda (pagamento misto).</summary>
public record PagamentoAplicado(FormaPagamento FormaPagamento, decimal Valor);
