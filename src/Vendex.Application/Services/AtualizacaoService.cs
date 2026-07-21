using System.Net.Http.Json;
using System.Text.Json;
using Vendex.Domain.Logging;

namespace Vendex.Application.Services;

public class AtualizacaoService : IAtualizacaoService
{
    private static readonly JsonSerializerOptions JsonOpcoes = new() { PropertyNameCaseInsensitive = true };
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(15) };

    public async Task<InfoAtualizacao?> ObterUltimaVersaoAsync()
    {
        try
        {
            var resposta = await Http.GetAsync($"{SupabaseFunctions.BaseUrl}/verificar-atualizacao");
            if (!resposta.IsSuccessStatusCode)
                return null;

            return await resposta.Content.ReadFromJsonAsync<InfoAtualizacao>(JsonOpcoes);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            Logger.Warn("Falha de conexão ao verificar atualização disponível.", ex);
            return null;
        }
    }
}
