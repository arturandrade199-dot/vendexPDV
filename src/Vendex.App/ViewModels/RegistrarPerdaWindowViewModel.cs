using System.Globalization;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.Application.Services;
using Vendex.Domain.Entities;

namespace Vendex.App.ViewModels;

/// <summary>Registra uma perda parcial ou total de um lote, informada como quantidade exata
/// ou como percentual do lote (mais prático que contar unidade por unidade na hora de jogar
/// fora uma fornada estragada) — a conversão percentual→quantidade é feita aqui, o serviço só
/// recebe a quantidade final resolvida.</summary>
public partial class RegistrarPerdaWindowViewModel : ObservableObject
{
    private static readonly CultureInfo CulturaBr = CultureInfo.GetCultureInfo("pt-BR");

    private readonly ILoteService _loteService;
    private readonly Lote _lote;

    public string ProdutoNome { get; }
    public string ValidadeFormatada { get; }
    public string RestamTexto { get; }

    [ObservableProperty] private bool usarPercentual;
    [ObservableProperty] private string quantidadeTexto = string.Empty;
    [ObservableProperty] private string percentualTexto = string.Empty;
    [ObservableProperty] private string motivo = string.Empty;
    [ObservableProperty] private string? mensagemErro;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PodeConfirmar))]
    private bool salvando;

    public bool PodeConfirmar => !Salvando;

    public event Action? Confirmado;

    public RegistrarPerdaWindowViewModel(ILoteService loteService, Lote lote)
    {
        _loteService = loteService;
        _lote = lote;

        ProdutoNome = lote.ProdutoVariante is null ? lote.Produto.Nome : $"{lote.Produto.Nome} — {lote.ProdutoVariante.Nome}";
        ValidadeFormatada = lote.DataValidade.ToString("dd/MM/yyyy", CulturaBr);
        RestamTexto = $"Restam {lote.QuantidadeAtual.ToString("0.###", CulturaBr)} de {lote.QuantidadeInicial.ToString("0.###", CulturaBr)} unidades";
    }

    [RelayCommand]
    private async Task ConfirmarAsync()
    {
        if (!TentarResolverQuantidade(out var quantidade, out var erro))
        {
            MensagemErro = erro;
            return;
        }

        MensagemErro = null;
        Salvando = true;
        try
        {
            await _loteService.RegistrarPerdaAsync(_lote.Id, quantidade, string.IsNullOrWhiteSpace(Motivo) ? null : Motivo.Trim());
            Confirmado?.Invoke();
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

    private bool TentarResolverQuantidade(out decimal quantidade, out string? erro)
    {
        quantidade = 0;
        erro = null;

        if (UsarPercentual)
        {
            if (!decimal.TryParse(PercentualTexto, out var percentual) || percentual <= 0 || percentual > 100)
            {
                erro = "Informe um percentual entre 0 e 100.";
                return false;
            }

            // Sempre limitado ao que ainda resta — "100% perdido" num lote já parcialmente
            // perdido antes significa "perder o resto", não um erro de conta.
            quantidade = Math.Min(percentual / 100m * _lote.QuantidadeInicial, _lote.QuantidadeAtual);
        }
        else
        {
            if (!decimal.TryParse(QuantidadeTexto, out quantidade) || quantidade <= 0)
            {
                erro = "Informe uma quantidade válida.";
                return false;
            }
        }

        if (quantidade > _lote.QuantidadeAtual)
        {
            erro = "Não é possível perder mais do que a quantidade restante no lote.";
            return false;
        }

        return true;
    }
}
