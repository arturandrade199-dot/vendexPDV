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

    public async Task<Cliente> AdicionarAsync(string nome, string? telefone, string? endereco = null, string? documento = null, decimal limiteCredito = 0, string? observacoes = null)
    {
        var cliente = new Cliente
        {
            Nome = nome,
            Telefone = telefone,
            Endereco = endereco,
            Documento = documento,
            LimiteCredito = limiteCredito,
            Observacoes = observacoes,
            DataCadastro = DateTime.Now
        };

        await _unitOfWork.Clientes.AdicionarAsync(cliente);
        await _unitOfWork.SalvarAlteracoesAsync();
        return cliente;
    }

    public async Task AtualizarAsync(int clienteId, string nome, string? telefone, string? endereco, string? documento, decimal limiteCredito, string? observacoes)
    {
        var cliente = await _unitOfWork.Clientes.ObterPorIdAsync(clienteId);
        if (cliente is null)
            return;

        cliente.Nome = nome;
        cliente.Telefone = telefone;
        cliente.Endereco = endereco;
        cliente.Documento = documento;
        cliente.LimiteCredito = limiteCredito;
        cliente.Observacoes = observacoes;

        _unitOfWork.Clientes.Atualizar(cliente);
        await _unitOfWork.SalvarAlteracoesAsync();
    }
}
