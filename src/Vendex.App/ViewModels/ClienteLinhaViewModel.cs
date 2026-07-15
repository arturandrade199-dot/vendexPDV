using System.Globalization;
using Vendex.Domain.Entities;

namespace Vendex.App.ViewModels;

public class ClienteLinhaViewModel
{
    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    public ClienteLinhaViewModel(Cliente cliente)
    {
        Id = cliente.Id;
        Nome = cliente.Nome;
        Telefone = string.IsNullOrWhiteSpace(cliente.Telefone) ? "—" : cliente.Telefone;
        Documento = string.IsNullOrWhiteSpace(cliente.Documento) ? "—" : cliente.Documento;
        Endereco = string.IsNullOrWhiteSpace(cliente.Endereco) ? "—" : cliente.Endereco;
        LimiteCreditoFormatado = cliente.LimiteCredito.ToString("C2", CulturaBr);
        DataCadastroFormatada = cliente.DataCadastro.ToString("dd/MM/yyyy");
    }

    public int Id { get; }
    public string Nome { get; }
    public string Telefone { get; }
    public string Documento { get; }
    public string Endereco { get; }
    public string LimiteCreditoFormatado { get; }
    public string DataCadastroFormatada { get; }
}
