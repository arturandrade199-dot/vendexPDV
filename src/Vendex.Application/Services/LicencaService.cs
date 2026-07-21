using System.Net.Http.Json;
using System.Text.Json;
using Vendex.Domain.Entities;
using Vendex.Domain.Enums;
using Vendex.Domain.Interfaces;
using Vendex.Licensing;

namespace Vendex.Application.Services;

public class LicencaService : ILicencaService
{
    private const string SupabaseFunctionsBaseUrl = "https://debjnxiglpiqrdtiewrw.supabase.co/functions/v1";

    private const int ToleranciaOfflineDias = 7;
    private static readonly TimeSpan ToleranciaRelogio = TimeSpan.FromMinutes(5);
    private static readonly JsonSerializerOptions JsonOpcoes = new() { PropertyNameCaseInsensitive = true };

    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(10) };

    private readonly IUnitOfWork _unitOfWork;

    public LicencaService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ResultadoAtivacao> AtivarAsync(string email)
    {
        email = email.Trim().ToLowerInvariant();
        var fingerprint = FingerprintProvider.ObterCodigoInstalacao();

        RespostaFunction? resposta;
        try
        {
            resposta = await ChamarFunctionAsync("ativar-licenca", email, fingerprint);
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            return new ResultadoAtivacao(false, "Não foi possível conectar. Verifique sua internet e tente novamente.");
        }

        if (resposta is null)
            return new ResultadoAtivacao(false, "Email não encontrado ou assinatura não está ativa — confirme sua compra.");

        if (!AssinaturaVerifier.VerificarAssinatura(resposta.Payload, resposta.Assinatura))
            return new ResultadoAtivacao(false, "Resposta do servidor inválida. Tente novamente mais tarde.");

        var dados = JsonSerializer.Deserialize<PayloadLicenca>(resposta.Payload, JsonOpcoes)
            ?? throw new InvalidOperationException("Payload de licença vazio.");

        if (dados.Fingerprint != fingerprint || !dados.Ativo)
            return new ResultadoAtivacao(false, "Não foi possível ativar nesta máquina.");

        var licenca = await ObterOuCriarLicencaAsync(fingerprint);
        licenca.Email = email;
        licenca.Status = StatusLicenca.Ativo;
        licenca.DataAtivacao ??= DateTime.Now;
        licenca.DataValidaAte = dados.ValidoAte;
        licenca.UltimaVerificacaoOnline = DateTime.Now;
        licenca.UltimaDataVista = DateTime.Now;

        _unitOfWork.Licencas.Atualizar(licenca);
        await _unitOfWork.SalvarAlteracoesAsync();

        return new ResultadoAtivacao(true, null);
    }

    public async Task<bool> VerificarELiberarAsync()
    {
        var fingerprint = FingerprintProvider.ObterCodigoInstalacao();
        var licenca = await ObterOuCriarLicencaAsync(fingerprint);

        // Detecção de relógio atrasado: a hora do sistema nunca pode "andar pra trás"
        // em relação à última vez que o app rodou (com uma tolerância pequena pra
        // fuso/ajustes automáticos legítimos).
        if (DateTime.Now < licenca.UltimaDataVista - ToleranciaRelogio)
        {
            licenca.Status = StatusLicenca.Bloqueado;
            _unitOfWork.Licencas.Atualizar(licenca);
            await _unitOfWork.SalvarAlteracoesAsync();
            return false;
        }

        licenca.UltimaDataVista = DateTime.Now;

        if (string.IsNullOrEmpty(licenca.Email))
        {
            await _unitOfWork.SalvarAlteracoesAsync();
            return false; // nunca ativado nesta máquina
        }

        try
        {
            var resposta = await ChamarFunctionAsync("verificar-licenca", licenca.Email, fingerprint);
            if (resposta is not null && AssinaturaVerifier.VerificarAssinatura(resposta.Payload, resposta.Assinatura))
            {
                var dados = JsonSerializer.Deserialize<PayloadLicenca>(resposta.Payload, JsonOpcoes);
                if (dados is not null && dados.Fingerprint == fingerprint)
                {
                    licenca.DataValidaAte = dados.ValidoAte;
                    licenca.UltimaVerificacaoOnline = DateTime.Now;
                    licenca.Status = dados.Ativo ? StatusLicenca.Ativo : StatusLicenca.Bloqueado;

                    _unitOfWork.Licencas.Atualizar(licenca);
                    await _unitOfWork.SalvarAlteracoesAsync();
                    return dados.Ativo;
                }
            }
        }
        catch (Exception ex) when (ex is HttpRequestException or TaskCanceledException)
        {
            // Sem internet — segue pra folga offline abaixo, usando a última
            // confirmação válida que o servidor já tinha dado.
        }

        _unitOfWork.Licencas.Atualizar(licenca);
        await _unitOfWork.SalvarAlteracoesAsync();

        return licenca.DataValidaAte is not null
            && DateTime.Now <= licenca.DataValidaAte.Value.AddDays(ToleranciaOfflineDias);
    }

    private async Task<Licenca> ObterOuCriarLicencaAsync(string fingerprint)
    {
        var licenca = await _unitOfWork.Licencas.ObterLicencaAtualAsync();
        if (licenca is not null)
            return licenca;

        licenca = new Licenca { CodigoInstalacao = fingerprint };
        await _unitOfWork.Licencas.AdicionarAsync(licenca);
        await _unitOfWork.SalvarAlteracoesAsync();
        return licenca;
    }

    private static async Task<RespostaFunction?> ChamarFunctionAsync(string nomeFuncao, string email, string fingerprint)
    {
        var resposta = await Http.PostAsJsonAsync($"{SupabaseFunctionsBaseUrl}/{nomeFuncao}", new { email, fingerprint });
        if (!resposta.IsSuccessStatusCode)
            return null;

        return await resposta.Content.ReadFromJsonAsync<RespostaFunction>(JsonOpcoes);
    }

    private record RespostaFunction(string Payload, string Assinatura);
    private record PayloadLicenca(string Fingerprint, bool Ativo, DateTime ValidoAte, DateTime EmitidoEm);
}
