using Vendex.Domain.Entities;

namespace Vendex.App.ViewModels;

public class FornecedorLinhaViewModel
{
    public FornecedorLinhaViewModel(Fornecedor fornecedor)
    {
        Id = fornecedor.Id;
        Nome = fornecedor.Nome;
        Telefone = string.IsNullOrWhiteSpace(fornecedor.Telefone) ? "—" : fornecedor.Telefone;
        Documento = string.IsNullOrWhiteSpace(fornecedor.Documento) ? "—" : fornecedor.Documento;
        Endereco = string.IsNullOrWhiteSpace(fornecedor.Endereco) ? "—" : fornecedor.Endereco;
        DataCadastroFormatada = fornecedor.DataCadastro.ToString("dd/MM/yyyy");
    }

    public int Id { get; }
    public string Nome { get; }
    public string Telefone { get; }
    public string Documento { get; }
    public string Endereco { get; }
    public string DataCadastroFormatada { get; }
}
