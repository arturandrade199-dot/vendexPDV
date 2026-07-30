using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.App.Navigation;
using Vendex.Application.Services;
using Vendex.Domain.Enums;

namespace Vendex.App.ViewModels;

public partial class ContasPagarViewModel : ObservableObject
{
    private const string NomeModulo = "Contas a Pagar";

    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IContaPagarService _contaPagarService;
    private readonly Func<NovaContaPagarWindow> _novaContaPagarWindowFactory;
    private readonly SessaoUsuario _sessao;

    public ObservableCollection<ContaPagarLinhaViewModel> Contas { get; } = new();

    public ObservableCollection<string> FormasPagamentoDisponiveis { get; } = new()
    {
        FormaPagamento.Dinheiro.ParaTexto(), FormaPagamento.CartaoCredito.ParaTexto(),
        FormaPagamento.CartaoDebito.ParaTexto(), FormaPagamento.Pix.ParaTexto(), FormaPagamento.Beneficios.ParaTexto()
    };

    [ObservableProperty] private string vencidosFormatado = "R$ 0,00";
    [ObservableProperty] private string vencemHojeFormatado = "R$ 0,00";
    [ObservableProperty] private string aVencerFormatado = "R$ 0,00";
    [ObservableProperty] private string pagosFormatado = "R$ 0,00";
    [ObservableProperty] private string totalPeriodoFormatado = "R$ 0,00";

    [ObservableProperty] private bool mostrarConfirmacaoPagamento;
    [ObservableProperty] private ContaPagarLinhaViewModel? contaParaPagar;
    [ObservableProperty] private string formaPagamentoSelecionada;

    public bool PodeCriar => _sessao.PodeCriar(NomeModulo);
    public bool PodeEditar => _sessao.PodeEditar(NomeModulo);

    public ContasPagarViewModel(IContaPagarService contaPagarService, Func<NovaContaPagarWindow> novaContaPagarWindowFactory, SessaoUsuario sessao)
    {
        _contaPagarService = contaPagarService;
        _novaContaPagarWindowFactory = novaContaPagarWindowFactory;
        _sessao = sessao;
        formaPagamentoSelecionada = FormasPagamentoDisponiveis[0];
        _ = CarregarAsync();
    }

    [RelayCommand]
    private async Task AdicionarAsync()
    {
        if (!PodeCriar) return;

        var janela = _novaContaPagarWindowFactory();
        if (janela.ShowDialog() == true)
        {
            await CarregarAsync();
        }
    }

    [RelayCommand]
    private void MarcarComoPago(ContaPagarLinhaViewModel linha)
    {
        if (!PodeEditar) return;

        ContaParaPagar = linha;
        FormaPagamentoSelecionada = FormasPagamentoDisponiveis[0];
        MostrarConfirmacaoPagamento = true;
    }

    [RelayCommand]
    private void CancelarPagamento() => MostrarConfirmacaoPagamento = false;

    [RelayCommand]
    private async Task ConfirmarPagamentoAsync()
    {
        if (ContaParaPagar is null)
            return;

        MostrarConfirmacaoPagamento = false;
        await _contaPagarService.MarcarComoPagoAsync(ContaParaPagar.Id, MapForma(FormaPagamentoSelecionada));
        ContaParaPagar = null;
        await CarregarAsync();
    }

    private static FormaPagamento MapForma(string texto) =>
        Enum.GetValues<FormaPagamento>().FirstOrDefault(f => f.ParaTexto() == texto, FormaPagamento.Dinheiro);

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
