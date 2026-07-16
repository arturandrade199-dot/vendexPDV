namespace Vendex.Application.Services;

public record ResultadoAtivacao(bool Sucesso, string? MensagemErro);

public interface ILicencaService
{
    /// <summary>Primeira ativação — vincula esta máquina ao email usado na compra.</summary>
    Task<ResultadoAtivacao> AtivarAsync(string email);

    /// <summary>Confere se a licença desta máquina continua válida: checa online se
    /// possível (e detecta relógio atrasado), cai na folga offline dos últimos dias se
    /// não tiver internet. Chamado na abertura do app e periodicamente em background
    /// (ver AgendadorLicenca).</summary>
    Task<bool> VerificarELiberarAsync();
}
