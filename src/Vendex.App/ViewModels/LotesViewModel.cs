using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.App.Navigation;
using Vendex.Application.Services;
using Vendex.Domain.Entities;
using Vendex.Domain.Enums;

namespace Vendex.App.ViewModels;

public partial class LotesViewModel : ObservableObject
{
    private const string NomeModulo = "Controle de Validade";

    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly ILoteService _loteService;
    private readonly Func<NovoLoteWindow> _novoLoteWindowFactory;
    private readonly Func<Lote, RegistrarPerdaWindow> _registrarPerdaWindowFactory;
    private readonly SessaoUsuario _sessao;
    private List<Lote> _todosLotes = new();

    public ObservableCollection<LoteLinhaViewModel> Lotes { get; } = new();
    public ObservableCollection<string> SituacoesDisponiveis { get; } = new()
    {
        "Todas", "Válido", "Vencendo", "Vencido", "Esgotado"
    };

    [ObservableProperty] private int vencidos;
    [ObservableProperty] private int vencemEm3Dias;
    [ObservableProperty] private int vencemEm7Dias;
    [ObservableProperty] private string perdidoNoPeriodoFormatado = "R$ 0,00";
    [ObservableProperty] private string termoBusca = string.Empty;
    [ObservableProperty] private string situacaoSelecionada = "Todas";

    public bool PodeCriar => _sessao.PodeCriar(NomeModulo);
    public bool PodeEditar => _sessao.PodeEditar(NomeModulo);
    public bool PodeExcluir => _sessao.PodeExcluir(NomeModulo);

    public LotesViewModel(
        ILoteService loteService, Func<NovoLoteWindow> novoLoteWindowFactory,
        Func<Lote, RegistrarPerdaWindow> registrarPerdaWindowFactory, SessaoUsuario sessao)
    {
        _loteService = loteService;
        _novoLoteWindowFactory = novoLoteWindowFactory;
        _registrarPerdaWindowFactory = registrarPerdaWindowFactory;
        _sessao = sessao;
        _ = CarregarAsync();
    }

    [RelayCommand]
    private async Task AdicionarAsync()
    {
        if (!PodeCriar) return;

        var janela = _novoLoteWindowFactory();
        if (janela.ShowDialog() == true)
        {
            await CarregarAsync();
        }
    }

    [RelayCommand]
    private async Task RegistrarPerdaAsync(LoteLinhaViewModel? linha)
    {
        if (!PodeEditar || linha is null) return;

        var lote = _todosLotes.FirstOrDefault(l => l.Id == linha.Id);
        if (lote is null)
            return;

        var janela = _registrarPerdaWindowFactory(lote);
        if (janela.ShowDialog() == true)
        {
            await CarregarAsync();
        }
    }

    [RelayCommand]
    private async Task RemoverAsync(LoteLinhaViewModel? linha)
    {
        if (!PodeExcluir || linha is null) return;

        var confirmar = MessageBox.Show(
            $"Remover o lote de \"{linha.ProdutoNome}\" (validade {linha.ValidadeFormatada})?",
            "Confirmar remoção",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);

        if (confirmar != MessageBoxResult.Yes)
            return;

        try
        {
            await _loteService.RemoverAsync(linha.Id);
            await CarregarAsync();
        }
        catch (InvalidOperationException ex)
        {
            MessageBox.Show(ex.Message, "Não foi possível remover", MessageBoxButton.OK, MessageBoxImage.Warning);
        }
    }

    partial void OnTermoBuscaChanged(string value) => AplicarFiltro();
    partial void OnSituacaoSelecionadaChanged(string value) => AplicarFiltro();

    private async Task CarregarAsync()
    {
        _todosLotes = (await _loteService.ListarAsync()).ToList();

        var resumo = await _loteService.ObterResumoAsync();
        Vencidos = resumo.Vencidos;
        VencemEm3Dias = resumo.VencemEm3Dias;
        VencemEm7Dias = resumo.VencemEm7Dias;
        PerdidoNoPeriodoFormatado = resumo.PerdidoNoPeriodo.ToString("C2", CulturaBr);

        AplicarFiltro();
    }

    private void AplicarFiltro()
    {
        var termo = TermoBusca.Trim();
        IEnumerable<Lote> filtrados = _todosLotes;

        if (!string.IsNullOrEmpty(termo))
            filtrados = filtrados.Where(l =>
                l.Produto.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                (l.ProdutoVariante?.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ?? false));

        filtrados = SituacaoSelecionada switch
        {
            "Válido" => filtrados.Where(l => l.SituacaoEfetiva() == SituacaoLote.Valido),
            "Vencendo" => filtrados.Where(l => l.SituacaoEfetiva() == SituacaoLote.Vencendo),
            "Vencido" => filtrados.Where(l => l.SituacaoEfetiva() == SituacaoLote.Vencido),
            "Esgotado" => filtrados.Where(l => l.SituacaoEfetiva() == SituacaoLote.Esgotado),
            _ => filtrados
        };

        Lotes.Clear();
        foreach (var lote in filtrados)
            Lotes.Add(new LoteLinhaViewModel(lote));
    }
}
