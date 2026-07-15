using Vendex.Domain.Entities;

namespace Vendex.Application.Services;

public interface IContaReceberService
{
    Task<IReadOnlyList<ContaReceber>> ListarAsync();
    Task<ResumoContasReceber> ObterResumoAsync();
    Task<ContaReceber> AdicionarAsync(int clienteId, string descricao, decimal valorTotal, DateTime dataVencimento);
    Task MarcarComoRecebidoAsync(int contaReceberId);
}
