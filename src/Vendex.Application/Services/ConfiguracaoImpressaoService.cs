using Vendex.Domain.Entities;
using Vendex.Domain.Interfaces;

namespace Vendex.Application.Services;

public class ConfiguracaoImpressaoService : IConfiguracaoImpressaoService
{
    private readonly IUnitOfWork _unitOfWork;

    public ConfiguracaoImpressaoService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public async Task<ConfiguracaoImpressao> ObterConfiguracaoAsync()
    {
        // ObterPorIdAsync (rastreado) em vez de ObterTodosAsync — mesmo motivo do
        // BackupService.ObterConfiguracaoAsync (DbContext singleton, evita tracking duplicado).
        var configuracao = await _unitOfWork.ConfiguracoesImpressao.ObterPorIdAsync(1);
        if (configuracao is not null)
            return configuracao;

        configuracao = new ConfiguracaoImpressao();
        await _unitOfWork.ConfiguracoesImpressao.AdicionarAsync(configuracao);
        await _unitOfWork.SalvarAlteracoesAsync();
        return configuracao;
    }

    public async Task SalvarConfiguracaoAsync(string? impressoraPadrao, bool imprimirAberturaCaixa, bool imprimirFechamentoCaixa, bool imprimirVenda)
    {
        var configuracao = await ObterConfiguracaoAsync();
        configuracao.ImpressoraPadrao = impressoraPadrao;
        configuracao.ImprimirAberturaCaixa = imprimirAberturaCaixa;
        configuracao.ImprimirFechamentoCaixa = imprimirFechamentoCaixa;
        configuracao.ImprimirVenda = imprimirVenda;

        _unitOfWork.ConfiguracoesImpressao.Atualizar(configuracao);
        await _unitOfWork.SalvarAlteracoesAsync();
    }
}
