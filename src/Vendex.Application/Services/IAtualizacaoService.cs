namespace Vendex.Application.Services;

public record InfoAtualizacao(string Versao, string UrlInstalador, string Sha256, string? Notas);

public interface IAtualizacaoService
{
    /// <summary>Consulta a Edge Function "verificar-atualizacao". Devolve null se não houver
    /// nenhuma versão publicada ou se a checagem falhar (ex.: sem internet) — nesses casos o
    /// chamador simplesmente não encontrou atualização, não é um erro pra propagar.</summary>
    Task<InfoAtualizacao?> ObterUltimaVersaoAsync();
}
