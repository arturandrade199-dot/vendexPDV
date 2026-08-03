using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.Application.Services;
using Vendex.Domain.Entities;

namespace Vendex.App.ViewModels;

/// <summary>Diálogo de busca de venda por período (padrão: hoje) e número/cliente — não fica
/// restrito ao cliente já selecionado na devolução, porque o operador pode querer localizar a
/// venda primeiro (ex: pelo número no cupom em mãos) e só então preencher o cliente a partir
/// dela.</summary>
public partial class SelecionarVendaWindowViewModel : ObservableObject
{
    private readonly IVendaService _vendaService;
    private List<Venda> _todasDoPeriodo = new();

    public ObservableCollection<Venda> Resultados { get; } = new();

    [ObservableProperty] private DateTime? dataInicio = DateTime.Today;
    [ObservableProperty] private DateTime? dataFim = DateTime.Today;
    [ObservableProperty] private string termoBusca = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PodeConfirmar))]
    private Venda? vendaSelecionada;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PodeBuscar))]
    private bool buscando;

    public bool PodeConfirmar => VendaSelecionada is not null;
    public bool PodeBuscar => !Buscando;

    public event Action? Confirmado;

    public SelecionarVendaWindowViewModel(IVendaService vendaService)
    {
        _vendaService = vendaService;
        _ = BuscarAsync();
    }

    partial void OnTermoBuscaChanged(string value) => AplicarFiltro();

    [RelayCommand]
    private async Task BuscarAsync()
    {
        Buscando = true;
        try
        {
            var inicio = (DataInicio ?? DateTime.Today).Date;
            var fim = (DataFim ?? DateTime.Today).Date.AddDays(1).AddTicks(-1);
            _todasDoPeriodo = (await _vendaService.ListarPorPeriodoAsync(inicio, fim))
                .OrderByDescending(v => v.DataHora)
                .ToList();
            AplicarFiltro();
        }
        finally
        {
            Buscando = false;
        }
    }

    private void AplicarFiltro()
    {
        var termo = TermoBusca.Trim();
        var filtradas = string.IsNullOrEmpty(termo)
            ? _todasDoPeriodo
            : _todasDoPeriodo.Where(v =>
                v.Id.ToString().Contains(termo) ||
                (v.Cliente?.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ?? false));

        Resultados.Clear();
        foreach (var venda in filtradas)
            Resultados.Add(venda);
    }

    [RelayCommand]
    private void Confirmar()
    {
        if (VendaSelecionada is not null)
            Confirmado?.Invoke();
    }
}
