using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.Application.Services;
using Vendex.Domain.Entities;

namespace Vendex.App.ViewModels;

public partial class NovaContaPagarViewModel : ObservableObject
{
    private readonly IContaPagarService _contaPagarService;
    private readonly IFornecedorService _fornecedorService;

    public ObservableCollection<Fornecedor> Fornecedores { get; } = new();

    [ObservableProperty] private string descricao = string.Empty;
    [ObservableProperty] private string categoria = string.Empty;
    [ObservableProperty] private string valorTotalTexto = string.Empty;
    [ObservableProperty] private DateTime dataVencimento = DateTime.Today;
    [ObservableProperty] private Fornecedor? fornecedorSelecionado;
    [ObservableProperty] private string? mensagemErro;

    public event Action? Salvo;

    public NovaContaPagarViewModel(IContaPagarService contaPagarService, IFornecedorService fornecedorService)
    {
        _contaPagarService = contaPagarService;
        _fornecedorService = fornecedorService;
        _ = CarregarFornecedoresAsync();
    }

    private async Task CarregarFornecedoresAsync()
    {
        var fornecedores = await _fornecedorService.ListarAsync();
        foreach (var fornecedor in fornecedores.OrderBy(f => f.Nome))
            Fornecedores.Add(fornecedor);
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
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
        await _contaPagarService.AdicionarAsync(Descricao.Trim(), Categoria.Trim(), valor, DataVencimento, FornecedorSelecionado?.Id);
        Salvo?.Invoke();
    }
}
