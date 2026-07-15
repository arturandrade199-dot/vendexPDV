using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.App.Navigation;
using Vendex.Application.Services;

namespace Vendex.App.ViewModels;

public partial class CaixaViewModel : ObservableObject
{
    public enum EstadoTela { Carregando, AbrirForm, AbrirRecibo, FecharResumo, FecharForm, FecharRecibo }

    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");
    private static readonly decimal[] ValoresNotas = { 200m, 100m, 50m, 20m, 10m, 5m, 2m };
    private static readonly decimal[] ValoresMoedas = { 1m, 0.50m, 0.25m, 0.10m, 0.05m };

    private readonly ICaixaService _caixaService;
    private readonly SessaoUsuario _sessao;

    public ObservableCollection<CedulaLinhaViewModel> LinhasNotas { get; } = new();
    public ObservableCollection<CedulaLinhaViewModel> LinhasMoedas { get; } = new();
    public ObservableCollection<CedulaLinhaViewModel> ReciboAberturaLinhas { get; } = new();
    public ObservableCollection<CedulaLinhaViewModel> ReciboFechamentoLinhas { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MostrarAbrirForm))]
    [NotifyPropertyChangedFor(nameof(MostrarAbrirRecibo))]
    [NotifyPropertyChangedFor(nameof(MostrarFecharResumo))]
    [NotifyPropertyChangedFor(nameof(MostrarFecharForm))]
    [NotifyPropertyChangedFor(nameof(MostrarFecharRecibo))]
    private EstadoTela estado = EstadoTela.Carregando;

    public bool MostrarAbrirForm => Estado == EstadoTela.AbrirForm;
    public bool MostrarAbrirRecibo => Estado == EstadoTela.AbrirRecibo;
    public bool MostrarFecharResumo => Estado == EstadoTela.FecharResumo;
    public bool MostrarFecharForm => Estado == EstadoTela.FecharForm;
    public bool MostrarFecharRecibo => Estado == EstadoTela.FecharRecibo;

    [ObservableProperty] private string totalContadoFormatado = "R$ 0,00";
    [ObservableProperty] private string? mensagemErro;

    [ObservableProperty] private string reciboAberturaDataFormatada = string.Empty;
    [ObservableProperty] private string reciboAberturaTotalFormatado = "R$ 0,00";

    [ObservableProperty] private string resumoAberturaDataFormatada = string.Empty;
    [ObservableProperty] private string resumoAberturaValorFormatado = "R$ 0,00";

    [ObservableProperty] private string fechamentoDataAberturaFormatada = string.Empty;
    [ObservableProperty] private string fechamentoDataFechamentoFormatada = string.Empty;
    [ObservableProperty] private string fechamentoValorAberturaFormatado = "R$ 0,00";
    [ObservableProperty] private string fechamentoValorFechamentoFormatado = "R$ 0,00";
    [ObservableProperty] private string fechamentoEsperadoFormatado = "R$ 0,00";
    [ObservableProperty] private string fechamentoDivergenciaFormatado = "R$ 0,00";
    [ObservableProperty] private string fechamentoFaturamentoFormatado = "R$ 0,00";
    [ObservableProperty] private string fechamentoCustoFormatado = "R$ 0,00";
    [ObservableProperty] private string fechamentoLucroFormatado = "R$ 0,00";

    public event Action? Concluido;

    public CaixaViewModel(ICaixaService caixaService, SessaoUsuario sessao)
    {
        _caixaService = caixaService;
        _sessao = sessao;

        foreach (var valor in ValoresNotas)
            AdicionarLinha(LinhasNotas, valor);
        foreach (var valor in ValoresMoedas)
            AdicionarLinha(LinhasMoedas, valor);

        _ = CarregarAsync();
    }

    private void AdicionarLinha(ObservableCollection<CedulaLinhaViewModel> colecao, decimal valor)
    {
        var linha = new CedulaLinhaViewModel(valor);
        linha.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName == nameof(CedulaLinhaViewModel.Quantidade))
                AtualizarTotalContado();
        };
        colecao.Add(linha);
    }

    private void AtualizarTotalContado()
    {
        var total = LinhasNotas.Concat(LinhasMoedas).Sum(l => l.Subtotal);
        TotalContadoFormatado = total.ToString("C2", CulturaBr);
    }

    private async Task CarregarAsync()
    {
        var caixaAberto = await _caixaService.ObterCaixaAbertoAsync();
        if (caixaAberto is null)
        {
            Estado = EstadoTela.AbrirForm;
            return;
        }

        ResumoAberturaDataFormatada = caixaAberto.DataAbertura.ToString("dd/MM/yyyy HH:mm", CulturaBr);
        ResumoAberturaValorFormatado = caixaAberto.ValorAberturaTotal.ToString("C2", CulturaBr);
        Estado = EstadoTela.FecharResumo;
    }

    [RelayCommand]
    private async Task AbrirAsync()
    {
        var todasLinhas = LinhasNotas.Concat(LinhasMoedas).ToList();
        if (todasLinhas.All(l => l.Quantidade == 0))
        {
            MensagemErro = "Informe a quantidade de ao menos uma cédula ou moeda.";
            return;
        }

        MensagemErro = null;
        var contagem = todasLinhas.Select(l => new ContagemCedula(l.Valor, l.Quantidade)).ToList();
        var caixa = await _caixaService.AbrirCaixaAsync(_sessao.UsuarioLogado!.Id, contagem);

        ReciboAberturaLinhas.Clear();
        foreach (var linha in todasLinhas.Where(l => l.Quantidade > 0))
            ReciboAberturaLinhas.Add(linha);

        ReciboAberturaDataFormatada = caixa.DataAbertura.ToString("dd/MM/yyyy HH:mm", CulturaBr);
        ReciboAberturaTotalFormatado = caixa.ValorAberturaTotal.ToString("C2", CulturaBr);
        Estado = EstadoTela.AbrirRecibo;
    }

    [RelayCommand]
    private void IniciarFechamento()
    {
        foreach (var linha in LinhasNotas.Concat(LinhasMoedas))
            linha.Quantidade = 0;

        TotalContadoFormatado = "R$ 0,00";
        MensagemErro = null;
        Estado = EstadoTela.FecharForm;
    }

    [RelayCommand]
    private async Task FecharAsync()
    {
        var todasLinhas = LinhasNotas.Concat(LinhasMoedas).ToList();
        if (todasLinhas.All(l => l.Quantidade == 0))
        {
            MensagemErro = "Informe a quantidade de ao menos uma cédula ou moeda.";
            return;
        }

        MensagemErro = null;
        var contagem = todasLinhas.Select(l => new ContagemCedula(l.Valor, l.Quantidade)).ToList();
        var resumo = await _caixaService.FecharCaixaAsync(_sessao.UsuarioLogado!.Id, contagem);

        ReciboFechamentoLinhas.Clear();
        foreach (var linha in todasLinhas.Where(l => l.Quantidade > 0))
            ReciboFechamentoLinhas.Add(linha);

        FechamentoDataAberturaFormatada = resumo.DataAbertura.ToString("dd/MM/yyyy HH:mm", CulturaBr);
        FechamentoDataFechamentoFormatada = resumo.DataFechamento.ToString("dd/MM/yyyy HH:mm", CulturaBr);
        FechamentoValorAberturaFormatado = resumo.ValorAberturaTotal.ToString("C2", CulturaBr);
        FechamentoValorFechamentoFormatado = resumo.ValorFechamentoTotal.ToString("C2", CulturaBr);
        FechamentoEsperadoFormatado = resumo.EsperadoEmCaixa.ToString("C2", CulturaBr);
        FechamentoDivergenciaFormatado = resumo.Divergencia.ToString("C2", CulturaBr);
        FechamentoFaturamentoFormatado = resumo.FaturamentoTotal.ToString("C2", CulturaBr);
        FechamentoCustoFormatado = resumo.CustoTotal.ToString("C2", CulturaBr);
        FechamentoLucroFormatado = resumo.LucroTotal.ToString("C2", CulturaBr);
        Estado = EstadoTela.FecharRecibo;
    }

    [RelayCommand]
    private void Concluir() => Concluido?.Invoke();
}
