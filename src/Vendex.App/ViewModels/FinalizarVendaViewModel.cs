using System.Collections.ObjectModel;
using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.Application.Services;
using Vendex.Domain.Entities;
using DominioFormaPagamento = Vendex.Domain.Enums.FormaPagamento;

namespace Vendex.App.ViewModels;

/// <summary>
/// Painel de pagamento embutido no PDV (substitui o grid enquanto ativo, não é mais uma
/// janela separada). Suporta pagamento misto: o operador vai adicionando linhas de
/// pagamento até o restante chegar a zero.
/// </summary>
public partial class FinalizarVendaViewModel : ObservableObject
{
    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    private const string TextoDinheiro = "Dinheiro";
    private const string TextoCartaoCredito = "Cartão de Crédito";
    private const string TextoCartaoDebito = "Cartão de Débito";
    private const string TextoPix = "PIX";
    private const string TextoBeneficios = "Benefícios";
    private const string TextoFiado = "Fiado (Contas a Receber)";

    private readonly IReadOnlyList<ItemCarrinhoViewModel> _itens;
    private readonly decimal _total;
    private readonly IVendaService _vendaService;
    private readonly IUsuarioService _usuarioService;
    private readonly IClienteService _clienteService;

    private decimal _restante;
    private decimal _trocoTotal;
    private int? _clienteIdFiado;
    private DateTime? _vencimentoFiado;

    public ObservableCollection<string> FormasPagamentoDisponiveis { get; } = new()
    {
        TextoDinheiro, TextoCartaoCredito, TextoCartaoDebito, TextoPix, TextoBeneficios, TextoFiado
    };

    public ObservableCollection<Cliente> Clientes { get; } = new();
    public ObservableCollection<LinhaPagamentoViewModel> Pagamentos { get; } = new();

    public string TotalFormatado { get; }

    [ObservableProperty] private string restanteFormatado;
    [ObservableProperty] private bool podeConfirmar;
    [ObservableProperty] private string? trocoTotalFormatado;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IsDinheiroSelecionado))]
    [NotifyPropertyChangedFor(nameof(IsFiadoSelecionado))]
    private string formaPagamentoSelecionada;

    [ObservableProperty] private string valorTexto = string.Empty;
    [ObservableProperty] private string previaTroco = string.Empty;
    [ObservableProperty] private Cliente? clienteSelecionado;
    [ObservableProperty] private DateTime vencimentoFiadoSelecionavel = DateTime.Today.AddDays(30);
    [ObservableProperty] private string novoClienteNome = string.Empty;
    [ObservableProperty] private string novoClienteTelefone = string.Empty;
    [ObservableProperty] private string? mensagemErro;

    public bool IsDinheiroSelecionado => FormaPagamentoSelecionada == TextoDinheiro;
    public bool IsFiadoSelecionado => FormaPagamentoSelecionada == TextoFiado;

    public ReciboVenda? Resultado { get; private set; }
    public event Action? Confirmado;
    public event Action? Voltar;

    public FinalizarVendaViewModel(
        IReadOnlyList<ItemCarrinhoViewModel> itens,
        IVendaService vendaService,
        IUsuarioService usuarioService,
        IClienteService clienteService)
    {
        _itens = itens;
        _total = itens.Sum(i => i.Subtotal);
        _restante = _total;
        _vendaService = vendaService;
        _usuarioService = usuarioService;
        _clienteService = clienteService;

        TotalFormatado = _total.ToString("C2", CulturaBr);
        restanteFormatado = _total.ToString("C2", CulturaBr);
        formaPagamentoSelecionada = TextoDinheiro;
        valorTexto = _total.ToString("0.00");

        _ = CarregarClientesAsync();
    }

    private async Task CarregarClientesAsync()
    {
        var clientes = await _clienteService.ListarAsync();
        Clientes.Clear();
        foreach (var cliente in clientes)
            Clientes.Add(cliente);
    }

    partial void OnFormaPagamentoSelecionadaChanged(string value)
    {
        ValorTexto = _restante.ToString("0.00");
        PreviaTroco = string.Empty;
        MensagemErro = null;
    }

    partial void OnValorTextoChanged(string value)
    {
        if (!IsDinheiroSelecionado)
        {
            PreviaTroco = string.Empty;
            return;
        }

        if (decimal.TryParse(value, out var informado) && informado > _restante)
            PreviaTroco = $"Troco: {(informado - _restante).ToString("C2", CulturaBr)}";
        else
            PreviaTroco = string.Empty;
    }

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
        MensagemErro = null;
    }

    [RelayCommand]
    private void AdicionarPagamento()
    {
        MensagemErro = null;

        if (!decimal.TryParse(ValorTexto, out var valorInformado) || valorInformado <= 0)
        {
            MensagemErro = "Informe um valor válido.";
            return;
        }

        var forma = MapForma(FormaPagamentoSelecionada);
        decimal valorAplicado;
        decimal? troco = null;
        string? clienteNome = null;

        if (forma == DominioFormaPagamento.Dinheiro)
        {
            valorAplicado = Math.Min(valorInformado, _restante);
            if (valorInformado > _restante)
                troco = valorInformado - _restante;
        }
        else if (forma == DominioFormaPagamento.Fiado)
        {
            if (ClienteSelecionado is null)
            {
                MensagemErro = "Selecione o cliente para a venda fiado.";
                return;
            }

            if (valorInformado > _restante)
            {
                MensagemErro = "O valor fiado não pode ser maior que o restante.";
                return;
            }

            valorAplicado = valorInformado;
            clienteNome = ClienteSelecionado.Nome;
            _clienteIdFiado = ClienteSelecionado.Id;
            _vencimentoFiado = VencimentoFiadoSelecionavel;
            FormasPagamentoDisponiveis.Remove(TextoFiado);
        }
        else
        {
            if (valorInformado > _restante)
            {
                MensagemErro = "O valor não pode ser maior que o restante.";
                return;
            }

            valorAplicado = valorInformado;
        }

        Pagamentos.Add(new LinhaPagamentoViewModel(forma, FormaPagamentoSelecionada, valorAplicado, troco, clienteNome));

        _restante -= valorAplicado;
        if (troco is > 0)
            _trocoTotal += troco.Value;

        RestanteFormatado = _restante.ToString("C2", CulturaBr);
        TrocoTotalFormatado = _trocoTotal > 0 ? _trocoTotal.ToString("C2", CulturaBr) : null;
        PodeConfirmar = _restante <= 0;

        if (FormasPagamentoDisponiveis.Count > 0)
            FormaPagamentoSelecionada = FormasPagamentoDisponiveis[0];
        ValorTexto = Math.Max(_restante, 0).ToString("0.00");
        PreviaTroco = string.Empty;
    }

    [RelayCommand]
    private void RemoverPagamento(LinhaPagamentoViewModel linha)
    {
        Pagamentos.Remove(linha);
        _restante += linha.Valor;
        if (linha.Troco is > 0)
            _trocoTotal -= linha.Troco.Value;

        if (linha.Forma == DominioFormaPagamento.Fiado && !FormasPagamentoDisponiveis.Contains(TextoFiado))
        {
            FormasPagamentoDisponiveis.Add(TextoFiado);
            _clienteIdFiado = null;
            _vencimentoFiado = null;
        }

        RestanteFormatado = _restante.ToString("C2", CulturaBr);
        TrocoTotalFormatado = _trocoTotal > 0 ? _trocoTotal.ToString("C2", CulturaBr) : null;
        PodeConfirmar = _restante <= 0 && Pagamentos.Count > 0;
        ValorTexto = Math.Max(_restante, 0).ToString("0.00");
    }

    [RelayCommand]
    private void VoltarAoCarrinho() => Voltar?.Invoke();

    [RelayCommand]
    private async Task ConfirmarAsync()
    {
        if (!PodeConfirmar)
        {
            MensagemErro = "Ainda falta cobrir o restante da venda.";
            return;
        }

        var usuarios = await _usuarioService.ListarAsync();
        var usuario = usuarios.FirstOrDefault();
        if (usuario is null)
        {
            MensagemErro = "Nenhum usuário cadastrado — não é possível registrar a venda.";
            return;
        }

        var itensDto = _itens
            .Select(i => new ItemCarrinho(i.ProdutoId, i.Quantidade, i.PrecoUnitario, i.PrecoCusto))
            .ToList();

        var pagamentosDto = Pagamentos
            .Select(p => new PagamentoAplicado(p.Forma, p.Valor))
            .ToList();

        var venda = await _vendaService.FinalizarVendaAsync(
            itensDto, pagamentosDto, usuario.Id, _clienteIdFiado, _vencimentoFiado);

        var clienteTexto = Pagamentos.FirstOrDefault(p => p.ClienteNome is not null)?.ClienteNome;

        Resultado = new ReciboVenda(
            venda.Id,
            venda.DataHora,
            _itens,
            TotalFormatado,
            Pagamentos.Select(p => p.TrocoFormatado is null
                ? $"{p.FormaTexto}: {p.ValorFormatado}"
                : $"{p.FormaTexto}: {p.ValorFormatado} (troco {p.TrocoFormatado})").ToList(),
            _trocoTotal > 0 ? _trocoTotal.ToString("C2", CulturaBr) : null,
            clienteTexto);

        Confirmado?.Invoke();
    }

    private static DominioFormaPagamento MapForma(string texto) => texto switch
    {
        TextoCartaoCredito => DominioFormaPagamento.CartaoCredito,
        TextoCartaoDebito => DominioFormaPagamento.CartaoDebito,
        TextoPix => DominioFormaPagamento.Pix,
        TextoBeneficios => DominioFormaPagamento.Beneficios,
        TextoFiado => DominioFormaPagamento.Fiado,
        _ => DominioFormaPagamento.Dinheiro
    };
}
