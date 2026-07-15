using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.Application.Services;
using Vendex.Domain.Entities;

namespace Vendex.App.ViewModels;

public partial class NovaContaReceberViewModel : ObservableObject
{
    private readonly IContaReceberService _contaReceberService;
    private readonly IClienteService _clienteService;

    public ObservableCollection<Cliente> Clientes { get; } = new();

    [ObservableProperty] private string descricao = string.Empty;
    [ObservableProperty] private string valorTotalTexto = string.Empty;
    [ObservableProperty] private DateTime dataVencimento = DateTime.Today;
    [ObservableProperty] private Cliente? clienteSelecionado;
    [ObservableProperty] private string? mensagemErro;

    public event Action? Salvo;

    public NovaContaReceberViewModel(IContaReceberService contaReceberService, IClienteService clienteService)
    {
        _contaReceberService = contaReceberService;
        _clienteService = clienteService;
        _ = CarregarClientesAsync();
    }

    private async Task CarregarClientesAsync()
    {
        var clientes = await _clienteService.ListarAsync();
        foreach (var cliente in clientes)
            Clientes.Add(cliente);
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        if (ClienteSelecionado is null)
        {
            MensagemErro = "Selecione o cliente.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Descricao))
        {
            MensagemErro = "Informe a descrição.";
            return;
        }

        if (!decimal.TryParse(ValorTotalTexto, out var valor) || valor <= 0)
        {
            MensagemErro = "Informe um valor válido.";
            return;
        }

        MensagemErro = null;
        await _contaReceberService.AdicionarAsync(ClienteSelecionado.Id, Descricao.Trim(), valor, DataVencimento);
        Salvo?.Invoke();
    }
}
