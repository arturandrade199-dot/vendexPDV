using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.Application.Services;
using Vendex.Domain.Entities;

namespace Vendex.App.ViewModels;

public partial class ProdutoWindowViewModel : ObservableObject
{
    private readonly IProdutoService _produtoService;
    private readonly int? _produtoId;

    [ObservableProperty] private string titulo = "Novo produto";
    [ObservableProperty] private string nome = string.Empty;
    [ObservableProperty] private string descricao = string.Empty;
    [ObservableProperty] private string codigoBarras = string.Empty;
    [ObservableProperty] private string precoCustoTexto = string.Empty;
    [ObservableProperty] private string precoVendaTexto = string.Empty;
    [ObservableProperty] private string estoqueTexto = "0";
    [ObservableProperty] private string? mensagemErro;

    public event Action? Salvo;

    public ProdutoWindowViewModel(IProdutoService produtoService, Produto? produtoExistente)
    {
        _produtoService = produtoService;

        if (produtoExistente is not null)
        {
            _produtoId = produtoExistente.Id;
            Titulo = "Editar produto";
            Nome = produtoExistente.Nome;
            Descricao = produtoExistente.Descricao ?? string.Empty;
            CodigoBarras = produtoExistente.CodigoBarras ?? string.Empty;
            PrecoCustoTexto = produtoExistente.PrecoCusto.ToString("0.00");
            PrecoVendaTexto = produtoExistente.PrecoVenda.ToString("0.00");
            EstoqueTexto = produtoExistente.EstoqueAtual.ToString();
        }
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        if (string.IsNullOrWhiteSpace(Nome))
        {
            MensagemErro = "Informe o nome do produto.";
            return;
        }

        if (!decimal.TryParse(PrecoCustoTexto, out var precoCusto) || precoCusto < 0)
        {
            MensagemErro = "Preço de custo inválido.";
            return;
        }

        if (!decimal.TryParse(PrecoVendaTexto, out var precoVenda) || precoVenda <= 0)
        {
            MensagemErro = "Preço de venda inválido.";
            return;
        }

        if (!int.TryParse(EstoqueTexto, out var estoque) || estoque < 0)
        {
            MensagemErro = "Estoque inválido.";
            return;
        }

        MensagemErro = null;
        var codigo = string.IsNullOrWhiteSpace(CodigoBarras) ? null : CodigoBarras.Trim();
        var desc = string.IsNullOrWhiteSpace(Descricao) ? null : Descricao.Trim();

        if (_produtoId is int id)
            await _produtoService.AtualizarAsync(id, Nome.Trim(), desc, codigo, precoCusto, precoVenda, estoque);
        else
            await _produtoService.AdicionarAsync(Nome.Trim(), desc, codigo, precoCusto, precoVenda, estoque);

        Salvo?.Invoke();
    }
}
