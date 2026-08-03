using Vendex.Domain.Entities;

namespace Vendex.Application.Services;

public interface ILoteService
{
    Task<IReadOnlyList<Lote>> ListarAsync();
    Task<ResumoLotes> ObterResumoAsync();

    Task<Lote> RegistrarAsync(
        int produtoId, int? produtoVarianteId, DateTime dataFabricacao, DateTime dataValidade,
        decimal quantidade, string? observacoes);

    Task<LotePerda> RegistrarPerdaAsync(int loteId, decimal quantidade, string? motivo);

    Task RemoverAsync(int loteId);
}
