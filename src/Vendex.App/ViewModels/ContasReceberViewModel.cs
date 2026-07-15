using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.App.Navigation;
using Vendex.Application.Services;

namespace Vendex.App.ViewModels;

public partial class ContasReceberViewModel : ObservableObject
{
    private const string NomeModulo = "Contas a Receber";

    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IContaReceberService _contaReceberService;
    private readonly Func<NovaContaReceberWindow> _novaContaReceberWindowFactory;
    private readonly SessaoUsuario _sessao;

    public ObservableCollection<ContaReceberLinhaViewModel> Contas { get; } = new();

    [ObservableProperty] private string vencidosFormatado = "R$ 0,00";
    [ObservableProperty] private string vencemHojeFormatado = "R$ 0,00";
    [ObservableProperty] private string aVencerFormatado = "R$ 0,00";
    [ObservableProperty] private string recebidosFormatado = "R$ 0,00";
    [ObservableProperty] private string totalPeriodoFormatado = "R$ 0,00";

    public bool PodeCriar => _sessao.PodeCriar(NomeModulo);
    public bool PodeEditar => _sessao.PodeEditar(NomeModulo);

    public ContasReceberViewModel(IContaReceberService contaReceberService, Func<NovaContaReceberWindow> novaContaReceberWindowFactory, SessaoUsuario sessao)
    {
        _contaReceberService = contaReceberService;
        _novaContaReceberWindowFactory = novaContaReceberWindowFactory;
        _sessao = sessao;
        _ = CarregarAsync();
    }

    [RelayCommand]
    private async Task AdicionarAsync()
    {
        if (!PodeCriar) return;

        var janela = _novaContaReceberWindowFactory();
        if (janela.ShowDialog() == true)
        {
            await CarregarAsync();
        }
    }

    [RelayCommand]
    private async Task MarcarComoRecebidoAsync(ContaReceberLinhaViewModel linha)
    {
        if (!PodeEditar) return;

        await _contaReceberService.MarcarComoRecebidoAsync(linha.Id);
        await CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        var contas = await _contaReceberService.ListarAsync();
        Contas.Clear();
        foreach (var conta in contas.OrderByDescending(c => c.DataVencimento))
            Contas.Add(new ContaReceberLinhaViewModel(conta));

        var resumo = await _contaReceberService.ObterResumoAsync();
        VencidosFormatado = resumo.Vencidos.ToString("C2", CulturaBr);
        VencemHojeFormatado = resumo.VencemHoje.ToString("C2", CulturaBr);
        AVencerFormatado = resumo.AVencer.ToString("C2", CulturaBr);
        RecebidosFormatado = resumo.Recebidos.ToString("C2", CulturaBr);
        TotalPeriodoFormatado = resumo.TotalPeriodo.ToString("C2", CulturaBr);
    }
}
