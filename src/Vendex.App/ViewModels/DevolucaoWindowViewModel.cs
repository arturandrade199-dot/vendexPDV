using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.App.Navigation;
using Vendex.Application.Services;
using Vendex.Domain.Entities;

namespace Vendex.App.ViewModels;

/// <summary>Devolução de mercadoria — avulsa (só cliente + produtos) ou vinculada a uma
/// venda do histórico do cliente selecionado. O estorno em dinheiro (saída de caixa) é
/// sempre uma escolha do operador na hora, não uma consequência automática.</summary>
public partial class DevolucaoWindowViewModel : ObservableObject
{
    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly IDevolucaoService _devolucaoService;
    private readonly IClienteService _clienteService;
    private readonly IVendaService _vendaService;
    private readonly SessaoUsuario _sessao;
    private readonly Func<IReadOnlyList<Cliente>, SelecionarClienteWindow> _selecionarClienteWindowFactory;
    private readonly Func<SelecionarVendaWindow> _selecionarVendaWindowFactory;
    private readonly Func<SelecionarProdutoWindow> _selecionarProdutoWindowFactory;

    // Evita que OnNumeroVendaTextoChanged reaja ao texto que o próprio OnVendaSelecionadaChanged
    // acabou de escrever ali (ex: ao selecionar pela lupa) — sem isso, o parse do número faria
    // uma segunda atribuição desnecessária de VendaSelecionada a cada seleção.
    private bool _sincronizandoNumeroVenda;

    public ObservableCollection<Cliente> Clientes { get; } = new();
    public ObservableCollection<ResultadoBuscaProduto> ResultadosBusca { get; } = new();
    public ObservableCollection<ItemDevolucaoViewModel> Itens { get; } = new();

    [ObservableProperty] private Cliente? clienteSelecionado;

    [ObservableProperty] private Venda? vendaSelecionada;

    [ObservableProperty] private string numeroVendaTexto = string.Empty;

    [ObservableProperty] private bool mostrarNovoCliente;
    [ObservableProperty] private string novoClienteNome = string.Empty;
    [ObservableProperty] private string novoClienteTelefone = string.Empty;

    [ObservableProperty] private string termoBusca = string.Empty;
    [ObservableProperty] private string quantidadeTexto = "1";

    [ObservableProperty] private string? motivo;
    [ObservableProperty] private bool estornarCaixa;
    [ObservableProperty] private string totalFormatado = "R$ 0,00";
    [ObservableProperty] private string? mensagemErro;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PodeConfirmar))]
    private bool salvando;

    public bool PodeConfirmar => !Salvando;

    public event Action? Salvo;

    public DevolucaoWindowViewModel(
        IDevolucaoService devolucaoService, IClienteService clienteService, IVendaService vendaService, SessaoUsuario sessao,
        Func<IReadOnlyList<Cliente>, SelecionarClienteWindow> selecionarClienteWindowFactory,
        Func<SelecionarVendaWindow> selecionarVendaWindowFactory,
        Func<SelecionarProdutoWindow> selecionarProdutoWindowFactory)
    {
        _devolucaoService = devolucaoService;
        _clienteService = clienteService;
        _vendaService = vendaService;
        _sessao = sessao;
        _selecionarClienteWindowFactory = selecionarClienteWindowFactory;
        _selecionarVendaWindowFactory = selecionarVendaWindowFactory;
        _selecionarProdutoWindowFactory = selecionarProdutoWindowFactory;
        _ = CarregarClientesAsync();
    }

    private async Task CarregarClientesAsync()
    {
        var clientes = await _clienteService.ListarAsync();
        Clientes.Clear();
        foreach (var cliente in clientes)
            Clientes.Add(cliente);
    }

    // Trocar de cliente manualmente invalida a venda escolhida antes — evita ficar com uma
    // venda de outro cliente vinculada por engano. AbrirSelecaoVenda/ResolverVendaPorNumeroAsync
    // contornam isso de propósito: setam ClienteSelecionado (o que passa por aqui e zera
    // VendaSelecionada) e IMEDIATAMENTE depois sobrescrevem com a venda escolhida.
    partial void OnClienteSelecionadoChanged(Cliente? value) => VendaSelecionada = null;

    partial void OnVendaSelecionadaChanged(Venda? value)
    {
        _sincronizandoNumeroVenda = true;
        NumeroVendaTexto = value is null ? string.Empty : value.Id.ToString();
        _sincronizandoNumeroVenda = false;
    }

    /// <summary>Permite digitar o número da venda direto, sem precisar abrir a lupa.</summary>
    partial void OnNumeroVendaTextoChanged(string value)
    {
        if (_sincronizandoNumeroVenda)
            return;

        if (int.TryParse(value, out var id))
            _ = ResolverVendaPorNumeroAsync(id);
    }

    private async Task ResolverVendaPorNumeroAsync(int id)
    {
        var venda = await _vendaService.ObterPorIdAsync(id);
        if (venda is null)
            return;

        SelecionarVendaEPreencherCliente(venda);
    }

    [RelayCommand]
    private void AbrirSelecaoCliente()
    {
        var janela = _selecionarClienteWindowFactory(Clientes);
        if (janela.ShowDialog() == true)
            ClienteSelecionado = janela.ClienteSelecionado;
    }

    [RelayCommand]
    private void AbrirSelecaoVenda()
    {
        var janela = _selecionarVendaWindowFactory();
        if (janela.ShowDialog() == true && janela.VendaSelecionada is not null)
            SelecionarVendaEPreencherCliente(janela.VendaSelecionada);
    }

    /// <summary>A busca de venda (por número ou pela lupa) não fica restrita ao cliente já
    /// selecionado — se a venda tiver cliente vinculado, ele preenche o campo Cliente sozinho.
    /// Os itens da venda já entram no carrinho de devolução direto, sem precisar de um passo
    /// manual extra — o operador remove o que não quiser devolver.</summary>
    private void SelecionarVendaEPreencherCliente(Venda venda)
    {
        if (venda.Cliente is not null)
            ClienteSelecionado = venda.Cliente;

        VendaSelecionada = venda;
        CarregarItensDaVenda();
    }

    [RelayCommand]
    private void AlternarNovoCliente() => MostrarNovoCliente = !MostrarNovoCliente;

    [RelayCommand]
    private async Task AdicionarClienteRapidoAsync()
    {
        if (string.IsNullOrWhiteSpace(NovoClienteNome))
        {
            MensagemErro = "Informe o nome do novo cliente.";
            return;
        }

        var telefone = string.IsNullOrWhiteSpace(NovoClienteTelefone) ? null : NovoClienteTelefone.Trim();
        var cliente = await _clienteService.AdicionarAsync(NovoClienteNome.Trim(), telefone);

        Clientes.Add(cliente);
        ClienteSelecionado = cliente;
        NovoClienteNome = string.Empty;
        NovoClienteTelefone = string.Empty;
        MostrarNovoCliente = false;
        MensagemErro = null;
    }

    private void CarregarItensDaVenda()
    {
        if (VendaSelecionada is null)
            return;

        foreach (var item in VendaSelecionada.Itens)
        {
            var nome = item.ProdutoVariante is null ? item.Produto.Nome : $"{item.Produto.Nome} — {item.ProdutoVariante.Nome}";
            AdicionarOuSomarItem(item.ProdutoId, item.ProdutoVarianteId, nome, item.PrecoUnitario, item.Quantidade);
        }
    }

    partial void OnTermoBuscaChanged(string value) => _ = BuscarAsync(value);

    private async Task BuscarAsync(string termo)
    {
        ResultadosBusca.Clear();
        if (string.IsNullOrWhiteSpace(termo))
            return;

        var resultados = await _vendaService.BuscarProdutosAsync(termo.Trim());
        foreach (var resultado in resultados.Take(8))
            ResultadosBusca.Add(resultado);
    }

    [RelayCommand]
    private void AbrirSelecaoProduto()
    {
        var janela = _selecionarProdutoWindowFactory();
        if (janela.ShowDialog() == true && janela.ProdutoSelecionado is not null)
            AdicionarProduto(janela.ProdutoSelecionado);
    }

    [RelayCommand]
    private void AdicionarProduto(ResultadoBuscaProduto resultado)
    {
        if (!decimal.TryParse(QuantidadeTexto, out var quantidade) || quantidade <= 0)
            quantidade = 1;

        AdicionarOuSomarItem(resultado.ProdutoId, resultado.VarianteId, resultado.NomeExibicao, resultado.PrecoVenda, quantidade);

        TermoBusca = string.Empty;
        QuantidadeTexto = "1";
        ResultadosBusca.Clear();
        MensagemErro = null;
    }

    [RelayCommand]
    private void AdicionarPrimeiroResultado()
    {
        if (ResultadosBusca.Count == 1)
            AdicionarProduto(ResultadosBusca[0]);
    }

    private void AdicionarOuSomarItem(int produtoId, int? produtoVarianteId, string nome, decimal precoUnitario, decimal quantidade)
    {
        var existente = Itens.FirstOrDefault(i => i.ProdutoId == produtoId && i.ProdutoVarianteId == produtoVarianteId);
        if (existente is not null)
        {
            existente.Quantidade += quantidade;
        }
        else
        {
            var item = new ItemDevolucaoViewModel(produtoId, produtoVarianteId, nome, precoUnitario, quantidade);
            item.PropertyChanged += (_, _) => AtualizarTotal();
            Itens.Add(item);
        }

        AtualizarTotal();
    }

    [RelayCommand]
    private void RemoverItem(ItemDevolucaoViewModel item)
    {
        Itens.Remove(item);
        AtualizarTotal();
    }

    private void AtualizarTotal()
    {
        var total = Itens.Sum(i => i.Subtotal);
        TotalFormatado = total.ToString("C2", CulturaBr);
    }

    [RelayCommand]
    private async Task ConfirmarAsync()
    {
        if (Itens.Count == 0)
        {
            MensagemErro = "Adicione ao menos um item para devolver.";
            return;
        }

        MensagemErro = null;
        Salvando = true;
        try
        {
            var itens = Itens
                .Select(i => new ItemDevolucao(i.ProdutoId, i.ProdutoVarianteId, i.Quantidade, i.PrecoUnitario))
                .ToList();

            await _devolucaoService.RegistrarAsync(
                ClienteSelecionado?.Id, VendaSelecionada?.Id, _sessao.UsuarioLogado!.Id, Motivo, itens, EstornarCaixa);

            Salvo?.Invoke();
        }
        catch (InvalidOperationException ex)
        {
            MensagemErro = ex.Message;
        }
        finally
        {
            Salvando = false;
        }
    }
}
