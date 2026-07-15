using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.Application.Services;
using Vendex.Domain.Entities;

namespace Vendex.App.ViewModels;

public partial class FornecedorWindowViewModel : ObservableObject
{
    private readonly IFornecedorService _fornecedorService;
    private readonly int? _fornecedorId;

    [ObservableProperty] private string titulo = "Novo fornecedor";
    [ObservableProperty] private string nome = string.Empty;
    [ObservableProperty] private string telefone = string.Empty;
    [ObservableProperty] private string endereco = string.Empty;
    [ObservableProperty] private string documento = string.Empty;
    [ObservableProperty] private string observacoes = string.Empty;
    [ObservableProperty] private string? mensagemErro;

    public event Action? Salvo;

    public FornecedorWindowViewModel(IFornecedorService fornecedorService, Fornecedor? fornecedorExistente)
    {
        _fornecedorService = fornecedorService;

        if (fornecedorExistente is not null)
        {
            _fornecedorId = fornecedorExistente.Id;
            Titulo = "Editar fornecedor";
            Nome = fornecedorExistente.Nome;
            Telefone = fornecedorExistente.Telefone ?? string.Empty;
            Endereco = fornecedorExistente.Endereco ?? string.Empty;
            Documento = fornecedorExistente.Documento ?? string.Empty;
            Observacoes = fornecedorExistente.Observacoes ?? string.Empty;
        }
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        if (string.IsNullOrWhiteSpace(Nome))
        {
            MensagemErro = "Informe o nome do fornecedor.";
            return;
        }

        MensagemErro = null;
        var telefone = string.IsNullOrWhiteSpace(Telefone) ? null : Telefone.Trim();
        var endereco = string.IsNullOrWhiteSpace(Endereco) ? null : Endereco.Trim();
        var documento = string.IsNullOrWhiteSpace(Documento) ? null : Documento.Trim();
        var observacoes = string.IsNullOrWhiteSpace(Observacoes) ? null : Observacoes.Trim();

        if (_fornecedorId is int id)
            await _fornecedorService.AtualizarAsync(id, Nome.Trim(), telefone, endereco, documento, observacoes);
        else
            await _fornecedorService.AdicionarAsync(Nome.Trim(), telefone, endereco, documento, observacoes);

        Salvo?.Invoke();
    }
}
