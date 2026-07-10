using Vendex.Domain.Enums;

namespace Vendex.Application.Services;

public interface IAuditoriaService
{
    Task RegistrarAsync(int usuarioId, string modulo, TipoAcaoAuditoria acao, string entidade, int? entidadeId, string descricao);
}
