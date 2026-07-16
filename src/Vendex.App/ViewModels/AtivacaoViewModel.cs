using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.Application.Services;

namespace Vendex.App.ViewModels;

public partial class AtivacaoViewModel : ObservableObject
{
    private readonly ILicencaService _licencaService;

    [ObservableProperty] private string email = string.Empty;
    [ObservableProperty] private string? mensagemErro;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PodeAtivar))]
    private bool ativando;

    public bool PodeAtivar => !Ativando;

    public event Action? Ativado;

    public AtivacaoViewModel(ILicencaService licencaService)
    {
        _licencaService = licencaService;
    }

    [RelayCommand]
    private async Task AtivarAsync()
    {
        if (string.IsNullOrWhiteSpace(Email))
        {
            MensagemErro = "Informe o email usado na compra.";
            return;
        }

        MensagemErro = null;
        Ativando = true;
        try
        {
            var resultado = await _licencaService.AtivarAsync(Email.Trim());
            if (resultado.Sucesso)
                Ativado?.Invoke();
            else
                MensagemErro = resultado.MensagemErro;
        }
        finally
        {
            Ativando = false;
        }
    }
}
