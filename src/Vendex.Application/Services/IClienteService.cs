using Vendex.Domain.Entities;

namespace Vendex.Application.Services;

public interface IClienteService
{
    Task<IReadOnlyList<Cliente>> ListarAsync();
    Task<Cliente> AdicionarAsync(string nome, string? telefone, string? endereco = null, string? documento = null, decimal limiteCredito = 0, string? observacoes = null);
    Task AtualizarAsync(int clienteId, string nome, string? telefone, string? endereco, string? documento, decimal limiteCredito, string? observacoes);
}
