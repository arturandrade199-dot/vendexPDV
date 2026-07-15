using Vendex.Domain.Entities;

namespace Vendex.Application.Services;

public interface IFornecedorService
{
    Task<IReadOnlyList<Fornecedor>> ListarAsync();
    Task<Fornecedor> AdicionarAsync(string nome, string? telefone, string? endereco, string? documento, string? observacoes);
    Task AtualizarAsync(int fornecedorId, string nome, string? telefone, string? endereco, string? documento, string? observacoes);
    Task RemoverAsync(int fornecedorId);
}
