using Vendex.Domain.Entities;

namespace Vendex.Application.Services;

public interface ICaixaService
{
    Task<Caixa?> ObterCaixaAbertoAsync();
    Task<Caixa> AbrirCaixaAsync(int usuarioId, IReadOnlyList<ContagemCedula> contagem);
    Task<ResumoFechamentoCaixa> FecharCaixaAsync(int usuarioId, IReadOnlyList<ContagemCedula> contagem);
}
