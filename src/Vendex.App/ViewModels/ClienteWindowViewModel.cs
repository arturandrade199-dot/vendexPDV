using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.Application.Services;
using Vendex.Domain.Entities;

namespace Vendex.App.ViewModels;

public partial class ClienteWindowViewModel : ObservableObject
{
    private readonly IClienteService _clienteService;
    private readonly int? _clienteId;

    [ObservableProperty] private string titulo = "Novo cliente";
    [ObservableProperty] private string nome = string.Empty;
    [ObservableProperty] private string telefone = string.Empty;
    [ObservableProperty] private string endereco = string.Empty;
    [ObservableProperty] private string documento = string.Empty;
    [ObservableProperty] private string limiteCreditoTexto = "0,00";
    [ObservableProperty] private string observacoes = string.Empty;
    [ObservableProperty] private string? mensagemErro;

    public event Action? Salvo;

    public ClienteWindowViewModel(IClienteService clienteService, Cliente? clienteExistente)
    {
        _clienteService = clienteService;

        if (clienteExistente is not null)
        {
            _clienteId = clienteExistente.Id;
            Titulo = "Editar cliente";
            Nome = clienteExistente.Nome;
            Telefone = clienteExistente.Telefone ?? string.Empty;
            Endereco = clienteExistente.Endereco ?? string.Empty;
            Documento = clienteExistente.Documento ?? string.Empty;
            LimiteCreditoTexto = clienteExistente.LimiteCredito.ToString("0.00");
            Observacoes = clienteExistente.Observacoes ?? string.Empty;
        }
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        if (string.IsNullOrWhiteSpace(Nome))
        {
            MensagemErro = "Informe o nome do cliente.";
            return;
        }

        if (!decimal.TryParse(LimiteCreditoTexto, out var limiteCredito) || limiteCredito < 0)
        {
            MensagemErro = "Limite de crédito inválido.";
            return;
        }

        MensagemErro = null;
        var telefone = string.IsNullOrWhiteSpace(Telefone) ? null : Telefone.Trim();
        var endereco = string.IsNullOrWhiteSpace(Endereco) ? null : Endereco.Trim();
        var documento = string.IsNullOrWhiteSpace(Documento) ? null : Documento.Trim();
        var observacoes = string.IsNullOrWhiteSpace(Observacoes) ? null : Observacoes.Trim();

        if (_clienteId is int id)
            await _clienteService.AtualizarAsync(id, Nome.Trim(), telefone, endereco, documento, limiteCredito, observacoes);
        else
            await _clienteService.AdicionarAsync(Nome.Trim(), telefone, endereco, documento, limiteCredito, observacoes);

        Salvo?.Invoke();
    }
}
