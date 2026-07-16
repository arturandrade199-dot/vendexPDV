using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.Application.Services;
using Vendex.Domain.Entities;
using Vendex.Domain.Enums;

namespace Vendex.App.ViewModels;

public partial class VendasViewModel : ObservableObject
{
    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IVendaService _vendaService;
    private readonly Func<ReciboVenda, ReciboWindow> _reciboWindowFactory;

    public ObservableCollection<VendaListaItemViewModel> Vendas { get; } = new();

    [ObservableProperty] private DateTime? dataInicio = DateTime.Today;
    [ObservableProperty] private DateTime? dataFim = DateTime.Today;
    [ObservableProperty] private int totalVendas;
    [ObservableProperty] private string totalFaturadoFormatado = "R$ 0,00";
    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PodeBuscar))]
    private bool buscando;

    public bool PodeBuscar => !Buscando;

    public VendasViewModel(IVendaService vendaService, Func<ReciboVenda, ReciboWindow> reciboWindowFactory)
    {
        _vendaService = vendaService;
        _reciboWindowFactory = reciboWindowFactory;
        _ = BuscarAsync();
    }

    [RelayCommand]
    private async Task BuscarAsync()
    {
        Buscando = true;
        try
        {
            var inicio = (DataInicio ?? DateTime.Today).Date;
            var fim = (DataFim ?? DateTime.Today).Date.AddDays(1).AddTicks(-1);
            var vendas = await _vendaService.ListarPorPeriodoAsync(inicio, fim);

            Vendas.Clear();
            foreach (var venda in vendas.OrderByDescending(v => v.DataHora))
                Vendas.Add(new VendaListaItemViewModel(venda));

            TotalVendas = vendas.Count;
            TotalFaturadoFormatado = vendas.Sum(v => v.ValorTotal).ToString("C2", CulturaBr);
        }
        finally
        {
            Buscando = false;
        }
    }

    [RelayCommand]
    private void VerDetalhes(VendaListaItemViewModel linha)
    {
        var recibo = MontarRecibo(linha.Venda);
        _reciboWindowFactory(recibo).ShowDialog();
    }

    private static ReciboVenda MontarRecibo(Venda venda)
    {
        var itens = venda.Itens
            .Select(i => new ItemCarrinhoViewModel(i.Produto.Nome, i.ProdutoId, i.PrecoUnitario, i.PrecoCustoUnitario, i.Quantidade))
            .ToList();

        // Troco não é persistido (VendaPagamento guarda só o valor aplicado à venda) — uma
        // venda antiga reimpressa nunca mostra linha de troco, ao contrário do cupom recém
        // emitido no PDV.
        var pagamentosTexto = venda.Pagamentos
            .Select(p => $"{p.FormaPagamento.ParaTexto()}: {p.Valor.ToString("C2", CulturaBr)}")
            .ToList();

        return new ReciboVenda(
            venda.Id, venda.DataHora, itens,
            venda.ValorTotal.ToString("C2", CulturaBr),
            pagamentosTexto, null, venda.Cliente?.Nome);
    }
}
