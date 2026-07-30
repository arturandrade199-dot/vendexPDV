namespace Vendex.Domain.Entities;

public class ConfiguracaoImpressao : EntidadeBase
{
    public string? ImpressoraPadrao { get; set; }
    public bool ImprimirAberturaCaixa { get; set; } = true;
    public bool ImprimirFechamentoCaixa { get; set; } = true;
    public bool ImprimirVenda { get; set; } = true;

    // Cupom de venda em layout estreito (impressora térmica não fiscal — Daruma, Elgin,
    // Epson etc.), como alternativa ao layout moderno de tela cheia. Dados da loja só
    // aparecem nesse layout (o moderno não tem cabeçalho de loja).
    public bool UsarLayoutTermico { get; set; }
    public string? NomeLoja { get; set; }
    public string? EnderecoLoja { get; set; }
    public string? CnpjLoja { get; set; }
}
