using Vendex.Domain.Entities;
using Vendex.Domain.Interfaces;

namespace Vendex.Application.Services;

public class FornecedorService : IFornecedorService
{
    private readonly IUnitOfWork _unitOfWork;

    public FornecedorService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<IReadOnlyList<Fornecedor>> ListarAsync() => _unitOfWork.Fornecedores.ObterTodosAsync();

    public async Task<Fornecedor> AdicionarAsync(string nome, string? telefone, string? endereco, string? documento, string? observacoes)
    {
        var fornecedor = new Fornecedor
        {
            Nome = nome,
            Telefone = telefone,
            Endereco = endereco,
            Documento = documento,
            Observacoes = observacoes
        };

        await _unitOfWork.Fornecedores.AdicionarAsync(fornecedor);
        await _unitOfWork.SalvarAlteracoesAsync();
        return fornecedor;
    }

    public async Task AtualizarAsync(int fornecedorId, string nome, string? telefone, string? endereco, string? documento, string? observacoes)
    {
        var fornecedor = await _unitOfWork.Fornecedores.ObterPorIdAsync(fornecedorId);
        if (fornecedor is null)
            return;

        fornecedor.Nome = nome;
        fornecedor.Telefone = telefone;
        fornecedor.Endereco = endereco;
        fornecedor.Documento = documento;
        fornecedor.Observacoes = observacoes;

        _unitOfWork.Fornecedores.Atualizar(fornecedor);
        await _unitOfWork.SalvarAlteracoesAsync();
    }

    public async Task RemoverAsync(int fornecedorId)
    {
        var fornecedor = await _unitOfWork.Fornecedores.ObterPorIdAsync(fornecedorId);
        if (fornecedor is null)
            return;

        _unitOfWork.Fornecedores.Remover(fornecedor);
        await _unitOfWork.SalvarAlteracoesAsync();
    }
}
