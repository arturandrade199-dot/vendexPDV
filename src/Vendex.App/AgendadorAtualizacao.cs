using System.Diagnostics;
using System.IO;
using System.Net.Http;
using System.Security.Cryptography;
using System.Windows;
using System.Windows.Threading;
using Vendex.Application.Services;
using Vendex.Domain.Logging;

namespace Vendex.App;

/// <summary>Verifica 1x por semana (+ 1x no startup) se há uma versão nova publicada. Nunca
/// aplica sozinho — só pergunta ao usuário, pra não interromper uma venda em andamento com um
/// reinício forçado. Reaproveita o instalador Inno Setup já existente em vez de um updater
/// separado: um .exe rodando não consegue se sobrescrever de qualquer forma.</summary>
public class AgendadorAtualizacao
{
    private readonly IAtualizacaoService _atualizacaoService;
    private readonly Dispatcher _dispatcherUi;
    private System.Threading.Timer? _timer;

    public AgendadorAtualizacao(IAtualizacaoService atualizacaoService)
    {
        _atualizacaoService = atualizacaoService;

        // Mesmo motivo dos outros agendadores: o Timer dispara numa thread do ThreadPool.
        _dispatcherUi = Dispatcher.CurrentDispatcher;
    }

    public void Iniciar()
    {
        _ = VerificarSilenciosamenteAsync();
        _timer = new System.Threading.Timer(
            _ => _dispatcherUi.InvokeAsync(VerificarSilenciosamenteAsync),
            null, TimeSpan.FromDays(7), TimeSpan.FromDays(7));
    }

    private async Task VerificarSilenciosamenteAsync()
    {
        try
        {
            var info = await ObterAtualizacaoDisponivelAsync();
            if (info is not null)
                await PerguntarEAplicarAsync(info);
        }
        catch (Exception ex)
        {
            Logger.Error("Falha na checagem de atualização.", ex);
        }
    }

    /// <summary>Chamado pelo botão "Verificar atualização agora" — ao contrário da checagem
    /// automática (silenciosa quando não há nada novo), aqui sempre devolve um texto pra
    /// mostrar na tela, mesmo quando já está tudo atualizado.</summary>
    public async Task<string> VerificarManualmenteAsync()
    {
        try
        {
            var info = await ObterAtualizacaoDisponivelAsync();
            if (info is null)
                return "Você já está com a versão mais recente.";

            await PerguntarEAplicarAsync(info);
            return string.Empty;
        }
        catch (Exception ex)
        {
            Logger.Error("Falha na checagem manual de atualização.", ex);
            return "Não foi possível verificar agora. Confira sua conexão e tente de novo.";
        }
    }

    /// <summary>Devolve as informações da versão disponível só se ela for mais nova que a
    /// instalada; null em qualquer outro caso (sem internet, nenhuma versão publicada ainda,
    /// ou já atualizado) — quem chama decide o que fazer com esse "nada a fazer".</summary>
    private async Task<InfoAtualizacao?> ObterAtualizacaoDisponivelAsync()
    {
        var info = await _atualizacaoService.ObterUltimaVersaoAsync();
        if (info is null || !Version.TryParse(info.Versao, out var versaoDisponivel))
            return null;

        var versaoAtual = System.Reflection.Assembly.GetExecutingAssembly().GetName().Version ?? new Version(0, 0, 0, 0);
        return versaoDisponivel > versaoAtual ? info : null;
    }

    private async Task PerguntarEAplicarAsync(InfoAtualizacao info)
    {
        var mensagem = $"Uma nova versão do Vendex PDV está disponível ({info.Versao}).";
        if (!string.IsNullOrWhiteSpace(info.Notas))
            mensagem += $"\n\n{info.Notas}";
        mensagem += "\n\nDeseja atualizar agora? O sistema vai fechar e reabrir sozinho.";

        var resposta = MessageBox.Show(mensagem, "Atualização disponível", MessageBoxButton.YesNo, MessageBoxImage.Information);
        if (resposta == MessageBoxResult.Yes)
            await BaixarEAplicarAsync(info);
    }

    private async Task BaixarEAplicarAsync(InfoAtualizacao info)
    {
        var caminhoTemp = Path.Combine(Path.GetTempPath(), $"VendexPDV-Setup-{info.Versao}.exe");

        using (var http = new HttpClient())
        using (var origem = await http.GetStreamAsync(info.UrlInstalador))
        using (var destino = File.Create(caminhoTemp))
        {
            await origem.CopyToAsync(destino);
        }

        var hashObtido = Convert.ToHexString(SHA256.HashData(File.ReadAllBytes(caminhoTemp)));
        if (!string.Equals(hashObtido, info.Sha256, StringComparison.OrdinalIgnoreCase))
        {
            // Instalador baixado não bate com o hash publicado — pode ser corrupção no
            // download ou (pior caso) um Storage/secret comprometido servindo outro arquivo.
            // Não executa de jeito nenhum nesse caso.
            Logger.Warn($"Hash do instalador baixado não confere (esperado {info.Sha256}, obtido {hashObtido}). Atualização cancelada.");
            File.Delete(caminhoTemp);
            MessageBox.Show(
                "Não foi possível confirmar a integridade do arquivo baixado. A atualização foi cancelada por segurança.",
                "Atualização cancelada", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }

        Logger.Info($"Atualização para {info.Versao} baixada e verificada — reiniciando para instalar.");

        // /VERYSILENT: sem tela do instalador. PrivilegesRequired=lowest no .iss já garante que
        // isso roda sem prompt de UAC, do jeito que o próprio usuário já instalou originalmente.
        Process.Start(new ProcessStartInfo(caminhoTemp, "/VERYSILENT /SUPPRESSMSGBOXES /NORESTART") { UseShellExecute = true });
        System.Windows.Application.Current.Shutdown();
    }
}
