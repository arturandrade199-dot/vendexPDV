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

    public ObservableCollection<ClienteLinhaViewModel> Clientes { get; } = new();

    [ObservableProperty] private int totalClientes;

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
    private async Task EditarAsync(ClienteLinhaViewModel linha)
    {
        if (!PodeEditar) return;

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

    private async Task CarregarAsync()
    {
        var clientes = await _clienteService.ListarAsync();
        Clientes.Clear();
        foreach (var cliente in clientes)
            Clientes.Add(new ClienteLinhaViewModel(cliente));

        TotalClientes = clientes.Count;
    }
}
