using Vendex.Domain.Entities;

namespace Vendex.Application.Services;

public interface IClienteService
{
    Task<IReadOnlyList<Cliente>> ListarAsync();
    Task<Cliente> AdicionarAsync(string nome, string? telefone);
}
