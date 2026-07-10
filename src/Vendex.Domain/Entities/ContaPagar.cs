using Vendex.Domain.Enums;

namespace Vendex.Domain.Entities;

public class ContaPagar : EntidadeBase
{
    public int? FornecedorId { get; set; }
    public Fornecedor? Fornecedor { get; set; }

    public string Descricao { get; set; } = string.Empty;
    public string Categoria { get; set; } = string.Empty;
    public decimal ValorTotal { get; set; }
    public DateTime DataLancamento { get; set; } = DateTime.Now;
    public DateTime DataVencimento { get; set; }
    public StatusContaFinanceira Status { get; set; } = StatusContaFinanceira.Aberto;

    public ICollection<ContaPagarPagamento> Pagamentos { get; set; } = new List<ContaPagarPagamento>();
}
