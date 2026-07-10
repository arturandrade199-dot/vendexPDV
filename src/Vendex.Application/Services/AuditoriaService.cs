using Vendex.Domain.Entities;
using Vendex.Domain.Enums;
using Vendex.Domain.Interfaces;

namespace Vendex.Application.Services;

public class AuditoriaService : IAuditoriaService
{
    private readonly IUnitOfWork _unitOfWork;

    public AuditoriaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task RegistrarAsync(int usuarioId, string modulo, TipoAcaoAuditoria acao, string entidade, int? entidadeId, string descricao)
    {
        var log = new LogAuditoria
        {
            UsuarioId = usuarioId,
            Modulo = modulo,
            Acao = acao,
            Entidade = entidade,
            EntidadeId = entidadeId,
            Descricao = descricao,
            DataHora = DateTime.Now
        };

        await _unitOfWork.LogsAuditoria.AdicionarAsync(log);
        await _unitOfWork.SalvarAlteracoesAsync();
    }
}
