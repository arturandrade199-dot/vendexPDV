namespace Vendex.Application.Services;

public interface IRelatorioProblemaService
{
    /// <summary>Envia um relatório de problema para o desenvolvedor (Edge Function
    /// "reportar-problema"). <paramref name="tipo"/> é "automatico" ou "manual".</summary>
    Task<(bool Sucesso, string? Erro)> EnviarAsync(string tipo, string? mensagemUsuario, string? conteudoLog);
}
