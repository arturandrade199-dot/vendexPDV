using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.App.Navigation;
using Vendex.Application.Services;
using Vendex.Domain.Enums;

namespace Vendex.App.ViewModels;

public partial class PdvViewModel : ObservableObject
{
    private const string NomeModulo = "PDV";

    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IVendaService _vendaService;
    private readonly ICaixaService _caixaService;
    private readonly IAuditoriaService _auditoriaService;
    private readonly SessaoUsuario _sessao;
    private readonly Func<CaixaWindow> _caixaWindowFactory;
    private readonly Func<IReadOnlyList<ItemCarrinhoViewModel>, FinalizarVendaViewModel> _finalizarVendaViewModelFactory;
    private readonly Func<string, string, AutorizacaoWindow> _autorizacaoWindowFactory;
    private readonly Func<DevolucaoWindow> _devolucaoWindowFactory;

    private TipoMovimentacaoCaixa _tipoMovimentacaoAtual;

    public ObservableCollection<ResultadoBuscaProduto> ResultadosBusca { get; } = new();
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

    [ObservableProperty] private bool mostrarConfirmacaoCancelamento;
    [ObservableProperty] private bool mostrarConfirmacaoSair;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MostrarConfirmacaoRemocaoItem))]
    [NotifyPropertyChangedFor(nameof(MensagemConfirmacaoRemocaoItem))]
    private ItemCarrinhoViewModel? itemParaRemover;

    public bool MostrarConfirmacaoRemocaoItem => ItemParaRemover is not null;
    public string MensagemConfirmacaoRemocaoItem => ItemParaRemover is null
        ? string.Empty
        : $"Remover \"{ItemParaRemover.Nome}\" da venda?";

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
        IAuditoriaService auditoriaService,
        SessaoUsuario sessao,
        Func<CaixaWindow> caixaWindowFactory,
        Func<IReadOnlyList<ItemCarrinhoViewModel>, FinalizarVendaViewModel> finalizarVendaViewModelFactory,
        Func<string, string, AutorizacaoWindow> autorizacaoWindowFactory,
        Func<DevolucaoWindow> devolucaoWindowFactory)
    {
        _vendaService = vendaService;
        _caixaService = caixaService;
        _auditoriaService = auditoriaService;
        _sessao = sessao;
        _caixaWindowFactory = caixaWindowFactory;
        _finalizarVendaViewModelFactory = finalizarVendaViewModelFactory;
        _autorizacaoWindowFactory = autorizacaoWindowFactory;
        _devolucaoWindowFactory = devolucaoWindowFactory;
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
    private void AdicionarProduto(ResultadoBuscaProduto resultado)
    {
        if (!decimal.TryParse(QuantidadeTexto, out var quantidade) || quantidade <= 0)
            quantidade = 1;

        var existente = Itens.FirstOrDefault(i => !i.Removido && i.ProdutoId == resultado.ProdutoId && i.ProdutoVarianteId == resultado.VarianteId);
        if (existente is not null)
            existente.Quantidade += quantidade;
        else
            Itens.Add(new ItemCarrinhoViewModel(resultado, quantidade));

        AtualizarResumo();
        TermoBusca = string.Empty;
        QuantidadeTexto = "1";
        ResultadosBusca.Clear();
        Mensagem = null;
    }

    [RelayCommand]
    private void AdicionarPrimeiroResultado()
    {
        // O foco costuma ficar em TxtBusca durante o uso normal do PDV, e o KeyBinding local
        // dele pra Enter é avaliado antes do KeyBinding de Enter da janela (mais próximo do
        // foco vence primeiro) — sem isso, o Enter dos diálogos de confirmação (Cancelar
        // Venda, Sair, Remover item) fica "engolido" aqui sem fazer nada quando o foco ainda
        // está na busca.
        if (MostrarConfirmacaoCancelamento || MostrarConfirmacaoRemocaoItem || MostrarConfirmacaoSair)
        {
            _ = ConfirmarDialogoAtivoAsync();
            return;
        }

        if (ResultadosBusca.Count == 1)
            AdicionarProduto(ResultadosBusca[0]);
    }

    [RelayCommand]
    private void RemoverItem(ItemCarrinhoViewModel item)
    {
        if (item.Removido)
            return;

        ItemParaRemover = item;
    }

    [RelayCommand]
    private void FecharConfirmacaoRemocaoItem() => ItemParaRemover = null;

    [RelayCommand]
    private async Task ConfirmarRemocaoItemAsync()
    {
        var item = ItemParaRemover;
        if (item is null)
            return;

        ItemParaRemover = null;

        await ExecutarComPermissaoAsync("remover um item da venda", () =>
        {
            item.Removido = true;
            AtualizarResumo();
            return Task.CompletedTask;
        });
    }

    [RelayCommand]
    private void FinalizarVenda()
    {
        // Já em pagamento: o F2 daqui é o atalho global da janela, mas quem deve tratá-lo
        // nesse estado é o PagamentoView (ConfirmarCommand). Reentrar aqui recriaria o
        // painel de pagamento do zero e perderia os lançamentos já feitos.
        if (EstaEmPagamento)
            return;

        var itensValidos = Itens.Where(i => !i.Removido).ToList();
        if (itensValidos.Count == 0)
        {
            Mensagem = "Adicione ao menos um produto antes de finalizar.";
            return;
        }

        var pagamento = _finalizarVendaViewModelFactory(itensValidos);
        pagamento.Voltar += () => PagamentoAtual = null;
        pagamento.Confirmado += () => OnVendaConfirmada(pagamento);
        PagamentoAtual = pagamento;
    }

    /// <summary>Disparado logo que um cupom novo fica pronto — usado pelo PdvWindow pra
    /// decidir se imprime automaticamente (config em Configurações), sem o ViewModel
    /// precisar conhecer PrintDialog/Visual (isso é responsabilidade da View).</summary>
    public event Action<ReciboVenda>? VendaFinalizada;

    /// <summary>Fecha a janela do PDV — disparado depois que o operador confirma a saída
    /// no popup do Esc. O ViewModel não conhece Window/Close(), então quem trata isso é o
    /// code-behind do PdvWindow.</summary>
    public event Action? SairSolicitado;

    [RelayCommand]
    private void SolicitarSaida()
    {
        // Esc fecha o que estiver aberto na hora (popup de confirmação de cancelamento/
        // remoção) antes de virar um pedido de saída da tela inteira.
        if (MostrarConfirmacaoCancelamento)
        {
            MostrarConfirmacaoCancelamento = false;
            return;
        }

        if (MostrarConfirmacaoRemocaoItem)
        {
            ItemParaRemover = null;
            return;
        }

        // Só permite sair da tela com o carrinho vazio — com itens no pedido, Esc não faz nada.
        if (Itens.Any(i => !i.Removido))
            return;

        MostrarConfirmacaoSair = !MostrarConfirmacaoSair;
    }

    [RelayCommand]
    private void FecharConfirmacaoSair() => MostrarConfirmacaoSair = false;

    [RelayCommand]
    private void ConfirmarSaida() => SairSolicitado?.Invoke();

    /// <summary>Atalho de Enter da janela: os diálogos de confirmação (cancelamento, sair,
    /// remover item) são overlays comuns — não Popup — então dá pra ter um único KeyBinding
    /// de Enter na janela que decide qual "Confirmar" chamar de acordo com o que está aberto
    /// no momento. Sem diálogo aberto, não faz nada.</summary>
    [RelayCommand]
    private async Task ConfirmarDialogoAtivoAsync()
    {
        if (MostrarConfirmacaoCancelamento)
        {
            await ConfirmarCancelamentoVendaAsync();
            return;
        }

        if (MostrarConfirmacaoRemocaoItem)
        {
            await ConfirmarRemocaoItemAsync();
            return;
        }

        if (MostrarConfirmacaoSair)
            ConfirmarSaida();
    }

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

        MostrarConfirmacaoCancelamento = true;
    }

    [RelayCommand]
    private void FecharConfirmacaoCancelamento() => MostrarConfirmacaoCancelamento = false;

    [RelayCommand]
    private async Task ConfirmarCancelamentoVendaAsync()
    {
        MostrarConfirmacaoCancelamento = false;

        await ExecutarComPermissaoAsync("cancelar a venda", async () =>
        {
            var quantidadeItens = Itens.Count(i => !i.Removido);
            Itens.Clear();
            AtualizarResumo();
            TermoBusca = string.Empty;
            ResultadosBusca.Clear();
            Mensagem = "Venda cancelada.";

            if (_sessao.UsuarioLogado is not null)
            {
                await _auditoriaService.RegistrarAsync(
                    _sessao.UsuarioLogado.Id, NomeModulo, TipoAcaoAuditoria.CancelamentoVenda,
                    "Venda", null, $"Venda cancelada no PDV com {quantidadeItens} item(ns).");
            }
        });
    }

    /// <summary>Só quem tem permissão de exclusão no módulo PDV pode cancelar a venda ou
    /// remover um item direto; sem ela, abre um diálogo pedindo login/senha de alguém que
    /// tenha — sem trocar o usuário da sessão atual.</summary>
    private async Task ExecutarComPermissaoAsync(string acaoDescricao, Func<Task> acao)
    {
        if (_sessao.PodeExcluir(NomeModulo))
        {
            await acao();
            return;
        }

        var janela = _autorizacaoWindowFactory(NomeModulo, $"Você não tem permissão para {acaoDescricao}.");
        if (janela.ShowDialog() == true)
            await acao();
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

    [RelayCommand]
    private async Task AbrirDevolucaoAsync()
    {
        // Mesma trava de permissão do cancelamento de venda — devolução mexe em estoque e,
        // opcionalmente, tira dinheiro do caixa, então não é uma ação "de qualquer um".
        await ExecutarComPermissaoAsync("registrar uma devolução", () =>
        {
            _devolucaoWindowFactory().ShowDialog();
            return Task.CompletedTask;
        });
    }

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
        var total = Itens.Where(i => !i.Removido).Sum(i => i.Subtotal);
        TotalFormatado = total.ToString("C2", CulturaBr);
        TemItens = Itens.Count > 0;
    }
}
