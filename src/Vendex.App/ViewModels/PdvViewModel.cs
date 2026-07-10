using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.Application.Services;
using Vendex.Domain.Entities;

namespace Vendex.App.ViewModels;

public partial class PdvViewModel : ObservableObject
{
    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IVendaService _vendaService;
    private readonly Func<IReadOnlyList<ItemCarrinhoViewModel>, FinalizarVendaViewModel> _finalizarVendaViewModelFactory;

    public ObservableCollection<Produto> ResultadosBusca { get; } = new();
    public ObservableCollection<ItemCarrinhoViewModel> Itens { get; } = new();

    [ObservableProperty] private string termoBusca = string.Empty;
    [ObservableProperty] private string quantidadeTexto = "1";
    [ObservableProperty] private string? mensagem;
    [ObservableProperty] private string totalFormatado = "R$ 0,00";
    [ObservableProperty] private bool temItens;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EstaEmPagamento))]
    [NotifyPropertyChangedFor(nameof(MostrarCarrinho))]
    [NotifyPropertyChangedFor(nameof(RotuloStatusCaixa))]
    private FinalizarVendaViewModel? pagamentoAtual;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(EstaComRecibo))]
    [NotifyPropertyChangedFor(nameof(MostrarCarrinho))]
    [NotifyPropertyChangedFor(nameof(RotuloStatusCaixa))]
    private ReciboVenda? reciboAtual;

    public bool EstaEmPagamento => PagamentoAtual is not null;
    public bool EstaComRecibo => ReciboAtual is not null;
    public bool MostrarCarrinho => !EstaEmPagamento && !EstaComRecibo;
    public string RotuloStatusCaixa => TemItens || EstaEmPagamento || EstaComRecibo ? "Caixa aberto" : "Caixa livre";

    public PdvViewModel(
        IVendaService vendaService,
        Func<IReadOnlyList<ItemCarrinhoViewModel>, FinalizarVendaViewModel> finalizarVendaViewModelFactory)
    {
        _vendaService = vendaService;
        _finalizarVendaViewModelFactory = finalizarVendaViewModelFactory;
    }

    partial void OnTermoBuscaChanged(string value) => _ = BuscarAsync(value);

    private async Task BuscarAsync(string termo)
    {
        ResultadosBusca.Clear();
        if (string.IsNullOrWhiteSpace(termo))
            return;

        var produtos = await _vendaService.BuscarProdutosAsync(termo.Trim());
        foreach (var produto in produtos.Take(8))
            ResultadosBusca.Add(produto);
    }

    [RelayCommand]
    private void AdicionarProduto(Produto produto)
    {
        if (!int.TryParse(QuantidadeTexto, out var quantidade) || quantidade <= 0)
            quantidade = 1;

        var existente = Itens.FirstOrDefault(i => i.ProdutoId == produto.Id);
        if (existente is not null)
            existente.Quantidade += quantidade;
        else
            Itens.Add(new ItemCarrinhoViewModel(produto, quantidade));

        AtualizarResumo();
        TermoBusca = string.Empty;
        QuantidadeTexto = "1";
        ResultadosBusca.Clear();
        Mensagem = null;
    }

    [RelayCommand]
    private void AdicionarPrimeiroResultado()
    {
        if (ResultadosBusca.Count == 1)
            AdicionarProduto(ResultadosBusca[0]);
    }

    [RelayCommand]
    private void RemoverItem(ItemCarrinhoViewModel item)
    {
        Itens.Remove(item);
        AtualizarResumo();
    }

    [RelayCommand]
    private void FinalizarVenda()
    {
        if (Itens.Count == 0)
        {
            Mensagem = "Adicione ao menos um produto antes de finalizar.";
            return;
        }

        var pagamento = _finalizarVendaViewModelFactory(Itens.ToList());
        pagamento.Voltar += () => PagamentoAtual = null;
        pagamento.Confirmado += () => OnVendaConfirmada(pagamento);
        PagamentoAtual = pagamento;
    }

    private void OnVendaConfirmada(FinalizarVendaViewModel pagamento)
    {
        var recibo = pagamento.Resultado;
        PagamentoAtual = null;
        Itens.Clear();
        AtualizarResumo();
        Mensagem = null;
        ReciboAtual = recibo;
    }

    [RelayCommand]
    private void NovaVenda() => ReciboAtual = null;

    [RelayCommand]
    private void CancelarVenda()
    {
        if (EstaEmPagamento)
        {
            PagamentoAtual = null;
            return;
        }

        Itens.Clear();
        AtualizarResumo();
        TermoBusca = string.Empty;
        ResultadosBusca.Clear();
        Mensagem = "Venda cancelada.";
    }

    private void AtualizarResumo()
    {
        var total = Itens.Sum(i => i.Subtotal);
        TotalFormatado = total.ToString("C2", CulturaBr);
        TemItens = Itens.Count > 0;
        OnPropertyChanged(nameof(RotuloStatusCaixa));
    }
}
