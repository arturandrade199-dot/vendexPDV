namespace Vendex.Domain.Entities;

public class Fornecedor : EntidadeBase
{
    public string Nome { get; set; } = string.Empty;
    public string? Telefone { get; set; }
    public string? Endereco { get; set; }
    public string? Documento { get; set; }
    public DateTime DataCadastro { get; set; } = DateTime.Now;
    public string? Observacoes { get; set; }
}
