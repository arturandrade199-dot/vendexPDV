using Vendex.Domain.Entities;
using Vendex.Domain.Enums;

namespace Vendex.Application.Services;

public interface ICaixaService
{
    Task<Caixa?> ObterCaixaAbertoAsync();
    Task<decimal> ObterValorEsperadoEmCaixaAsync();
    Task<Caixa> AbrirCaixaAsync(int usuarioId, IReadOnlyList<ContagemCedula> contagem);
    Task<ResumoFechamentoCaixa> FecharCaixaAsync(int usuarioId, IReadOnlyList<ContagemCedula> contagem);
    Task RegistrarMovimentacaoAsync(int usuarioId, TipoMovimentacaoCaixa tipo, decimal valor, string motivo);
}
