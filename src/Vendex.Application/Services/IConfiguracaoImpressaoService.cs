using Vendex.Domain.Entities;

namespace Vendex.Application.Services;

public interface IConfiguracaoImpressaoService
{
    Task<ConfiguracaoImpressao> ObterConfiguracaoAsync();
    Task SalvarConfiguracaoAsync(
        string? impressoraPadrao, bool imprimirAberturaCaixa, bool imprimirFechamentoCaixa, bool imprimirVenda,
        bool usarLayoutTermico, string? nomeLoja, string? enderecoLoja, string? cnpjLoja);
}
