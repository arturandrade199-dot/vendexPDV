using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.Application.Services;
using Vendex.Domain.Entities;
using Vendex.Domain.Enums;

namespace Vendex.App.ViewModels;

public partial class ProdutoWindowViewModel : ObservableObject
{
    private readonly IProdutoService _produtoService;
    private readonly int? _produtoId;

    /// <summary>Trava o recálculo em cadeia enquanto um dos três campos (custo/venda/%) é
    /// atualizado a partir dos outros dois — sem isso, cada set dispara o handler dos outros
    /// campos e entra em loop.</summary>
    private bool _recalculando;

    [ObservableProperty] private string titulo = "Novo produto";
    [ObservableProperty] private string nome = string.Empty;
    [ObservableProperty] private string codigoBarras = string.Empty;
    [ObservableProperty] private string precoCustoTexto = string.Empty;
    [ObservableProperty] private string precoVendaTexto = string.Empty;
    [ObservableProperty] private string percentualTexto = string.Empty;
    [ObservableProperty] private string estoqueTexto = "0";
    [ObservableProperty] private string unidadeSelecionada;
    [ObservableProperty] private bool temGrade;
    [ObservableProperty] private string? mensagemErro;

    public ObservableCollection<string> UnidadesDisponiveis { get; } =
        new(Enum.GetValues<UnidadeMedida>().Select(u => u.ParaTexto()));

    public ObservableCollection<VarianteLinhaViewModel> Variantes { get; } = new();

    public event Action? Salvo;

    public ProdutoWindowViewModel(IProdutoService produtoService, Produto? produtoExistente)
    {
        _produtoService = produtoService;
        unidadeSelecionada = UnidadeMedida.UN.ParaTexto();

        if (produtoExistente is not null)
        {
            _produtoId = produtoExistente.Id;
            Titulo = "Editar produto";
            Nome = produtoExistente.Nome;
            CodigoBarras = produtoExistente.CodigoBarras ?? string.Empty;
            PrecoCustoTexto = produtoExistente.PrecoCusto.ToString("0.00");
            PrecoVendaTexto = produtoExistente.PrecoVenda.ToString("0.00");
            if (produtoExistente.PrecoCusto > 0)
                PercentualTexto = ((produtoExistente.PrecoVenda - produtoExistente.PrecoCusto) / produtoExistente.PrecoCusto * 100).ToString("0.##");
            EstoqueTexto = produtoExistente.EstoqueAtual.ToString("0.###");
            UnidadeSelecionada = produtoExistente.UnidadeMedida.ParaTexto();
            TemGrade = produtoExistente.TemGrade;

            foreach (var variante in produtoExistente.Variantes)
                Variantes.Add(new VarianteLinhaViewModel(variante.Id, variante.Nome, variante.CodigoBarras, variante.EstoqueAtual));
        }
    }

    partial void OnPrecoCustoTextoChanged(string value)
    {
        if (_recalculando) return;

        if (decimal.TryParse(PercentualTexto, out _))
            RecalcularVendaAPartirDoPercentual();
        else
            RecalcularPercentualAPartirDaVenda();
    }

    partial void OnPercentualTextoChanged(string value)
    {
        if (_recalculando) return;
        RecalcularVendaAPartirDoPercentual();
    }

    partial void OnPrecoVendaTextoChanged(string value)
    {
        if (_recalculando) return;
        RecalcularPercentualAPartirDaVenda();
    }

    private void RecalcularVendaAPartirDoPercentual()
    {
        if (!decimal.TryParse(PrecoCustoTexto, out var custo) || !decimal.TryParse(PercentualTexto, out var percentual))
            return;

        _recalculando = true;
        PrecoVendaTexto = (custo * (1 + percentual / 100)).ToString("0.00");
        _recalculando = false;
    }

    private void RecalcularPercentualAPartirDaVenda()
    {
        if (!decimal.TryParse(PrecoCustoTexto, out var custo) || custo <= 0 || !decimal.TryParse(PrecoVendaTexto, out var venda))
            return;

        _recalculando = true;
        PercentualTexto = ((venda - custo) / custo * 100).ToString("0.##");
        _recalculando = false;
    }

    [RelayCommand]
    private void AdicionarVariante() => Variantes.Add(new VarianteLinhaViewModel());

    [RelayCommand]
    private void RemoverVariante(VarianteLinhaViewModel linha) => Variantes.Remove(linha);

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

        var unidadeMedida = Enum.GetValues<UnidadeMedida>().First(u => u.ParaTexto() == UnidadeSelecionada);
        var variantesInput = new List<VarianteInput>();
        decimal estoque = 0;

        if (TemGrade)
        {
            if (Variantes.Count == 0)
            {
                MensagemErro = "Adicione ao menos uma variação para um produto com grade.";
                return;
            }

            foreach (var linha in Variantes)
            {
                if (string.IsNullOrWhiteSpace(linha.Nome))
                {
                    MensagemErro = "Informe o nome de todas as variações.";
                    return;
                }

                if (!decimal.TryParse(linha.EstoqueTexto, out var estoqueVariante) || estoqueVariante < 0)
                {
                    MensagemErro = $"Estoque inválido na variação \"{linha.Nome}\".";
                    return;
                }

                var codigoVariante = string.IsNullOrWhiteSpace(linha.CodigoBarras) ? null : linha.CodigoBarras.Trim();
                variantesInput.Add(new VarianteInput(linha.Id, linha.Nome.Trim(), codigoVariante, estoqueVariante));
            }
        }
        else if (!decimal.TryParse(EstoqueTexto, out estoque) || estoque < 0)
        {
            MensagemErro = "Estoque inválido.";
            return;
        }

        MensagemErro = null;
        var codigo = string.IsNullOrWhiteSpace(CodigoBarras) ? null : CodigoBarras.Trim();
        var input = new ProdutoInput(Nome.Trim(), codigo, precoCusto, precoVenda, estoque, unidadeMedida, TemGrade, variantesInput);

        if (_produtoId is int id)
            await _produtoService.AtualizarAsync(id, input);
        else
            await _produtoService.AdicionarAsync(input);

        Salvo?.Invoke();
    }
}
