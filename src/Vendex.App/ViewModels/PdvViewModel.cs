using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.App.Navigation;
using Vendex.Application.Services;
using Vendex.Domain.Entities;
using Vendex.Domain.Enums;

namespace Vendex.App.ViewModels;

public partial class PdvViewModel : ObservableObject
{
    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IVendaService _vendaService;
    private readonly ICaixaService _caixaService;
    private readonly SessaoUsuario _sessao;
    private readonly Func<CaixaWindow> _caixaWindowFactory;
    private readonly Func<IReadOnlyList<ItemCarrinhoViewModel>, FinalizarVendaViewModel> _finalizarVendaViewModelFactory;

    private TipoMovimentacaoCaixa _tipoMovimentacaoAtual;

    public ObservableCollection<Produto> ResultadosBusca { get; } = new();
    public ObservableCollection<ItemCarrinhoViewModel> Itens { get; } = new();

    [ObservableProperty] private string termoBusca = string.Empty;
    [ObservableProperty] private string quantidadeTexto = "1";
    [ObservableProperty] private string? mensagem;
    [ObservableProperty] private string totalFormatado = "R$ 0,00";
    [ObservableProperty] private bool temItens;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(RotuloStatusCaixa))]
    [NotifyPropertyChangedFor(nameof(MostrarCarrinho))]
    [NotifyPropertyChangedFor(nameof(MostrarBloqueioSemCaixa))]
    private bool caixaAberto;

    [ObservableProperty] private bool mostrarPainelMovimentacao;
    [ObservableProperty] private string tituloMovimentacao = string.Empty;
    [ObservableProperty] private string valorMovimentacaoTexto = string.Empty;
    [ObservableProperty] private string motivoMovimentacao = string.Empty;
    [ObservableProperty] private string? mensagemErroMovimentacao;

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
    public bool MostrarCarrinho => CaixaAberto && !EstaEmPagamento && !EstaComRecibo;
    public bool MostrarBloqueioSemCaixa => !CaixaAberto && !EstaEmPagamento && !EstaComRecibo;
    public string RotuloStatusCaixa => CaixaAberto ? "Caixa aberto" : "Nenhum caixa aberto";

    public PdvViewModel(
        IVendaService vendaService,
        ICaixaService caixaService,
        SessaoUsuario sessao,
        Func<CaixaWindow> caixaWindowFactory,
        Func<IReadOnlyList<ItemCarrinhoViewModel>, FinalizarVendaViewModel> finalizarVendaViewModelFactory)
    {
        _vendaService = vendaService;
        _caixaService = caixaService;
        _sessao = sessao;
        _caixaWindowFactory = caixaWindowFactory;
        _finalizarVendaViewModelFactory = finalizarVendaViewModelFactory;
        _ = AtualizarStatusCaixaAsync();
    }

    private async Task AtualizarStatusCaixaAsync()
    {
        var caixa = await _caixaService.ObterCaixaAbertoAsync();
        CaixaAberto = caixa is not null;
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

    /// <summary>Disparado logo que um cupom novo fica pronto — usado pelo PdvWindow pra
    /// decidir se imprime automaticamente (config em Configurações), sem o ViewModel
    /// precisar conhecer PrintDialog/Visual (isso é responsabilidade da View).</summary>
    public event Action<ReciboVenda>? VendaFinalizada;

    private void OnVendaConfirmada(FinalizarVendaViewModel pagamento)
    {
        var recibo = pagamento.Resultado;
        PagamentoAtual = null;
        Itens.Clear();
        AtualizarResumo();
        Mensagem = null;
        ReciboAtual = recibo;

        if (recibo is not null)
            VendaFinalizada?.Invoke(recibo);
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

    [RelayCommand]
    private async Task AbrirCaixaJanelaAsync()
    {
        _caixaWindowFactory().ShowDialog();
        await AtualizarStatusCaixaAsync();
    }

    [RelayCommand]
    private void AbrirSangria() => AbrirPainelMovimentacao(TipoMovimentacaoCaixa.Sangria);

    [RelayCommand]
    private void AbrirSuprimento() => AbrirPainelMovimentacao(TipoMovimentacaoCaixa.Reforco);

    private void AbrirPainelMovimentacao(TipoMovimentacaoCaixa tipo)
    {
        _tipoMovimentacaoAtual = tipo;
        TituloMovimentacao = tipo == TipoMovimentacaoCaixa.Sangria ? "Sangria" : "Suprimento";
        ValorMovimentacaoTexto = string.Empty;
        MotivoMovimentacao = string.Empty;
        MensagemErroMovimentacao = null;
        MostrarPainelMovimentacao = true;
    }

    [RelayCommand]
    private void CancelarMovimentacao() => MostrarPainelMovimentacao = false;

    [RelayCommand]
    private async Task ConfirmarMovimentacaoAsync()
    {
        if (!decimal.TryParse(ValorMovimentacaoTexto, out var valor) || valor <= 0)
        {
            MensagemErroMovimentacao = "Informe um valor válido maior que zero.";
            return;
        }

        if (string.IsNullOrWhiteSpace(MotivoMovimentacao))
        {
            MensagemErroMovimentacao = "Informe o motivo.";
            return;
        }

        try
        {
            await _caixaService.RegistrarMovimentacaoAsync(_sessao.UsuarioLogado!.Id, _tipoMovimentacaoAtual, valor, MotivoMovimentacao.Trim());
            MostrarPainelMovimentacao = false;
        }
        catch (InvalidOperationException ex)
        {
            MensagemErroMovimentacao = ex.Message;
        }
    }

    private void AtualizarResumo()
    {
        var total = Itens.Sum(i => i.Subtotal);
        TotalFormatado = total.ToString("C2", CulturaBr);
        TemItens = Itens.Count > 0;
    }
}
