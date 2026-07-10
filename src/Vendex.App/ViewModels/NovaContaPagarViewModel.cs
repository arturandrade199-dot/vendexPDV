using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.Application.Services;

namespace Vendex.App.ViewModels;

public partial class NovaContaPagarViewModel : ObservableObject
{
    private readonly IContaPagarService _contaPagarService;

    [ObservableProperty] private string descricao = string.Empty;
    [ObservableProperty] private string categoria = string.Empty;
    [ObservableProperty] private string valorTotalTexto = string.Empty;
    [ObservableProperty] private DateTime dataVencimento = DateTime.Today;
    [ObservableProperty] private string? mensagemErro;

    public event Action? Salvo;

    public NovaContaPagarViewModel(IContaPagarService contaPagarService)
    {
        _contaPagarService = contaPagarService;
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
        await _contaPagarService.AdicionarAsync(Descricao.Trim(), Categoria.Trim(), valor, DataVencimento);
        Salvo?.Invoke();
    }
}
