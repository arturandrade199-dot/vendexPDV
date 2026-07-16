namespace Vendex.Domain.Entities;

public class ConfiguracaoImpressao : EntidadeBase
{
    public string? ImpressoraPadrao { get; set; }
    public bool ImprimirAberturaCaixa { get; set; } = true;
    public bool ImprimirFechamentoCaixa { get; set; } = true;
    public bool ImprimirVenda { get; set; } = true;
}
