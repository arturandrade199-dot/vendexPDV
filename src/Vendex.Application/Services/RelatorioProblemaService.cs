using System.Net.Http.Json;
using Vendex.Domain.Interfaces;
using Vendex.Domain.Logging;
using Vendex.Licensing;

namespace Vendex.Application.Services;

public class RelatorioProblemaService : IRelatorioProblemaService
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(15) };

    private readonly IUnitOfWork _unitOfWork;

    public RelatorioProblemaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<(bool Sucesso, string? Erro)> EnviarAsync(string tipo, string? mensagemUsuario, string? conteudoLog)
    {
        var fingerprint = FingerprintProvider.ObterCodigoInstalacao();
        var licenca = await _unitOfWork.Licencas.ObterLicencaAtualAsync();

        try
        {
            var resposta = await Http.PostAsJsonAsync($"{SupabaseFunctions.BaseUrl}/reportar-problema", new
            {
                fingerprint,
                email = licenca?.Email,
                tipo,
                mensagem = mensagemUsuario,
                log = conteudoLog,
            });

            if (resposta.IsSuccessStatusCode)
                return (true, null);

            // 429 = já mandou um relatório recente pra esse fingerprint (limite do lado do
            // servidor) — evita que um erro em loop vire uma enxurrada de emails.
            if (resposta.StatusCode == System.Net.HttpStatusCode.TooManyRequests)
                return (false, "Já foi enviado um relatório recentemente. Aguarde alguns minutos e tente de novo.");

            return (false, $"Servidor retornou {(int)resposta.StatusCode}.");
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            Logger.Warn("Falha de conexão ao enviar relatório de problema.", ex);
            return (false, "Não foi possível conectar. Verifique sua internet e tente novamente.");
        }
    }
}
