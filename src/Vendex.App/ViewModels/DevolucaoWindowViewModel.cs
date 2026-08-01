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

    public ObservableCollection<Cliente> Clientes { get; } = new();
    public ObservableCollection<Venda> VendasCliente { get; } = new();
    public ObservableCollection<ResultadoBuscaProduto> ResultadosBusca { get; } = new();
    public ObservableCollection<ItemDevolucaoViewModel> Itens { get; } = new();

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PodeCarregarItensDaVenda))]
    private Cliente? clienteSelecionado;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PodeCarregarItensDaVenda))]
    private Venda? vendaSelecionada;

    public bool PodeCarregarItensDaVenda => VendaSelecionada is not null;

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

    public DevolucaoWindowViewModel(IDevolucaoService devolucaoService, IClienteService clienteService, IVendaService vendaService, SessaoUsuario sessao)
    {
        _devolucaoService = devolucaoService;
        _clienteService = clienteService;
        _vendaService = vendaService;
        _sessao = sessao;
        _ = CarregarClientesAsync();
    }

    private async Task CarregarClientesAsync()
    {
        var clientes = await _clienteService.ListarAsync();
        Clientes.Clear();
        foreach (var cliente in clientes)
            Clientes.Add(cliente);
    }

    partial void OnClienteSelecionadoChanged(Cliente? value)
    {
        VendaSelecionada = null;
        VendasCliente.Clear();
        if (value is not null)
            _ = CarregarVendasDoClienteAsync(value.Id);
    }

    private async Task CarregarVendasDoClienteAsync(int clienteId)
    {
        var vendas = await _devolucaoService.ListarVendasPorClienteAsync(clienteId);
        VendasCliente.Clear();
        foreach (var venda in vendas)
            VendasCliente.Add(venda);
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

    [RelayCommand]
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
        if (ClienteSelecionado is null)
        {
            MensagemErro = "Selecione o cliente.";
            return;
        }

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
                ClienteSelecionado.Id, VendaSelecionada?.Id, _sessao.UsuarioLogado!.Id, Motivo, itens, EstornarCaixa);

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
