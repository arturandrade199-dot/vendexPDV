using Vendex.Domain.Entities;
using Vendex.Domain.Interfaces;

namespace Vendex.Application.Services;

public class ClienteService : IClienteService
{
    private readonly IUnitOfWork _unitOfWork;

    public ClienteService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<IReadOnlyList<Cliente>> ListarAsync()
    {
        var clientes = await _unitOfWork.Clientes.ObterTodosAsync();
        return clientes.OrderBy(c => c.Nome).ToList();
    }

    public async Task<Cliente> AdicionarAsync(string nome, string? telefone)
    {
        var cliente = new Cliente
        {
            Nome = nome,
            Telefone = telefone,
            DataCadastro = DateTime.Now
        };

        await _unitOfWork.Clientes.AdicionarAsync(cliente);
        await _unitOfWork.SalvarAlteracoesAsync();
        return cliente;
    }
}
