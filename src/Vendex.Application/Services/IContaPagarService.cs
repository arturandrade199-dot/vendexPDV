using Vendex.Domain.Entities;

namespace Vendex.Application.Services;

public interface IContaPagarService
{
    Task<IReadOnlyList<ContaPagar>> ListarAsync();
    Task<ResumoContasPagar> ObterResumoAsync();
    Task<ContaPagar> AdicionarAsync(string descricao, string categoria, decimal valorTotal, DateTime dataVencimento);
    Task MarcarComoPagoAsync(int contaPagarId);
}
