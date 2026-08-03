using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.App.Navigation;
using Vendex.Application.Services;
using Vendex.Domain.Entities;

namespace Vendex.App.ViewModels;

public partial class ClientesViewModel : ObservableObject
{
    private const string NomeModulo = "Clientes";

    private readonly IClienteService _clienteService;
    private readonly Func<Cliente?, ClienteWindow> _clienteWindowFactory;
    private readonly SessaoUsuario _sessao;
    private List<Cliente> _todosClientes = new();

    public ObservableCollection<ClienteLinhaViewModel> Clientes { get; } = new();

    [ObservableProperty] private int totalClientes;
    [ObservableProperty] private ClienteLinhaViewModel? itemSelecionado;
    [ObservableProperty] private string termoBusca = string.Empty;

    public bool PodeCriar => _sessao.PodeCriar(NomeModulo);
    public bool PodeEditar => _sessao.PodeEditar(NomeModulo);

    public ClientesViewModel(IClienteService clienteService, Func<Cliente?, ClienteWindow> clienteWindowFactory, SessaoUsuario sessao)
    {
        _clienteService = clienteService;
        _clienteWindowFactory = clienteWindowFactory;
        _sessao = sessao;
        _ = CarregarAsync();
    }

    [RelayCommand]
    private async Task AdicionarAsync()
    {
        if (!PodeCriar) return;

        var janela = _clienteWindowFactory(null);
        if (janela.ShowDialog() == true)
        {
            await CarregarAsync();
        }
    }

    [RelayCommand]
    private async Task EditarAsync(ClienteLinhaViewModel? linha)
    {
        if (!PodeEditar || linha is null) return;

        var clientes = await _clienteService.ListarAsync();
        var alvo = clientes.FirstOrDefault(c => c.Id == linha.Id);
        if (alvo is null)
            return;

        var janela = _clienteWindowFactory(alvo);
        if (janela.ShowDialog() == true)
        {
            await CarregarAsync();
        }
    }

    partial void OnTermoBuscaChanged(string value) => AplicarFiltro();

    private async Task CarregarAsync()
    {
        _todosClientes = (await _clienteService.ListarAsync()).ToList();
        TotalClientes = _todosClientes.Count;
        AplicarFiltro();
    }

    /// <summary>Filtra em memória — a lista de clientes cabe inteira em RAM sem esforço, então
    /// não vale a pena um parâmetro de busca ida-e-volta ao banco a cada tecla digitada.</summary>
    private void AplicarFiltro()
    {
        var termo = TermoBusca.Trim();
        var filtrados = string.IsNullOrEmpty(termo)
            ? _todosClientes
            : _todosClientes.Where(c =>
                c.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                (c.Telefone?.Contains(termo, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (c.Documento?.Contains(termo, StringComparison.OrdinalIgnoreCase) ?? false));

        Clientes.Clear();
        foreach (var cliente in filtrados)
            Clientes.Add(new ClienteLinhaViewModel(cliente));
    }
}
