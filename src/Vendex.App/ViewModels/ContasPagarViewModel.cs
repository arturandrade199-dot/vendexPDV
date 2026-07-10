using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.Application.Services;

namespace Vendex.App.ViewModels;

public partial class ContasPagarViewModel : ObservableObject
{
    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IContaPagarService _contaPagarService;
    private readonly Func<NovaContaPagarWindow> _novaContaPagarWindowFactory;

    public ObservableCollection<ContaPagarLinhaViewModel> Contas { get; } = new();

    [ObservableProperty] private string vencidosFormatado = "R$ 0,00";
    [ObservableProperty] private string vencemHojeFormatado = "R$ 0,00";
    [ObservableProperty] private string aVencerFormatado = "R$ 0,00";
    [ObservableProperty] private string pagosFormatado = "R$ 0,00";
    [ObservableProperty] private string totalPeriodoFormatado = "R$ 0,00";

    public ContasPagarViewModel(IContaPagarService contaPagarService, Func<NovaContaPagarWindow> novaContaPagarWindowFactory)
    {
        _contaPagarService = contaPagarService;
        _novaContaPagarWindowFactory = novaContaPagarWindowFactory;
        _ = CarregarAsync();
    }

    [RelayCommand]
    private async Task AdicionarAsync()
    {
        var janela = _novaContaPagarWindowFactory();
        if (janela.ShowDialog() == true)
        {
            await CarregarAsync();
        }
    }

    [RelayCommand]
    private async Task MarcarComoPagoAsync(ContaPagarLinhaViewModel linha)
    {
        await _contaPagarService.MarcarComoPagoAsync(linha.Id);
        await CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        var contas = await _contaPagarService.ListarAsync();
        Contas.Clear();
        foreach (var conta in contas.OrderByDescending(c => c.DataVencimento))
            Contas.Add(new ContaPagarLinhaViewModel(conta));

        var resumo = await _contaPagarService.ObterResumoAsync();
        VencidosFormatado = resumo.Vencidos.ToString("C2", CulturaBr);
        VencemHojeFormatado = resumo.VencemHoje.ToString("C2", CulturaBr);
        AVencerFormatado = resumo.AVencer.ToString("C2", CulturaBr);
        PagosFormatado = resumo.Pagos.ToString("C2", CulturaBr);
        TotalPeriodoFormatado = resumo.TotalPeriodo.ToString("C2", CulturaBr);
    }
}
