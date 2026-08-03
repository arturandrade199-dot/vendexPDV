using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.App.Navigation;
using Vendex.Application.Services;
using Vendex.Domain.Entities;
using Vendex.Domain.Enums;

namespace Vendex.App.ViewModels;

public partial class ContasPagarViewModel : ObservableObject
{
    private const string NomeModulo = "Contas a Pagar";

    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IContaPagarService _contaPagarService;
    private readonly Func<NovaContaPagarWindow> _novaContaPagarWindowFactory;
    private readonly SessaoUsuario _sessao;
    private List<ContaPagar> _todasContas = new();

    public ObservableCollection<ContaPagarLinhaViewModel> Contas { get; } = new();

    public ObservableCollection<string> FormasPagamentoDisponiveis { get; } = new()
    {
        FormaPagamento.Dinheiro.ParaTexto(), FormaPagamento.CartaoCredito.ParaTexto(),
        FormaPagamento.CartaoDebito.ParaTexto(), FormaPagamento.Pix.ParaTexto(), FormaPagamento.Beneficios.ParaTexto()
    };

    public ObservableCollection<string> SituacoesDisponiveis { get; } = new()
    {
        "Todas", "Em aberto", "Parcial", "Pago", "Atrasado"
    };

    [ObservableProperty] private string vencidosFormatado = "R$ 0,00";
    [ObservableProperty] private string vencemHojeFormatado = "R$ 0,00";
    [ObservableProperty] private string aVencerFormatado = "R$ 0,00";
    [ObservableProperty] private string pagosFormatado = "R$ 0,00";
    [ObservableProperty] private string totalPeriodoFormatado = "R$ 0,00";
    [ObservableProperty] private string termoBusca = string.Empty;
    [ObservableProperty] private string situacaoSelecionada = "Todas";

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
    private void CancelarPagamento()
    {
        MostrarConfirmacaoPagamento = false;
        ContaParaPagar = null;
    }

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

    partial void OnTermoBuscaChanged(string value) => AplicarFiltro();
    partial void OnSituacaoSelecionadaChanged(string value) => AplicarFiltro();

    private async Task CarregarAsync()
    {
        _todasContas = (await _contaPagarService.ListarAsync()).OrderByDescending(c => c.DataVencimento).ToList();

        var resumo = await _contaPagarService.ObterResumoAsync();
        VencidosFormatado = resumo.Vencidos.ToString("C2", CulturaBr);
        VencemHojeFormatado = resumo.VencemHoje.ToString("C2", CulturaBr);
        AVencerFormatado = resumo.AVencer.ToString("C2", CulturaBr);
        PagosFormatado = resumo.Pagos.ToString("C2", CulturaBr);
        TotalPeriodoFormatado = resumo.TotalPeriodo.ToString("C2", CulturaBr);

        AplicarFiltro();
    }

    private void AplicarFiltro()
    {
        var termo = TermoBusca.Trim();
        IEnumerable<ContaPagar> filtradas = _todasContas;

        if (!string.IsNullOrEmpty(termo))
            filtradas = filtradas.Where(c =>
                c.Descricao.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                (c.Fornecedor?.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ?? false));

        filtradas = SituacaoSelecionada switch
        {
            "Em aberto" => filtradas.Where(c => c.StatusEfetivo == StatusContaFinanceira.Aberto),
            "Parcial" => filtradas.Where(c => c.StatusEfetivo == StatusContaFinanceira.Parcial),
            "Pago" => filtradas.Where(c => c.StatusEfetivo == StatusContaFinanceira.Pago),
            "Atrasado" => filtradas.Where(c => c.StatusEfetivo == StatusContaFinanceira.Atrasado),
            _ => filtradas
        };

        Contas.Clear();
        foreach (var conta in filtradas)
            Contas.Add(new ContaPagarLinhaViewModel(conta));
    }
}
