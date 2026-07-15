using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.App.Navigation;
using Vendex.Application.Services;
using Vendex.Domain.Enums;

namespace Vendex.App.ViewModels;

public partial class MovimentacaoCaixaWindowViewModel : ObservableObject
{
    private readonly ICaixaService _caixaService;
    private readonly SessaoUsuario _sessao;
    private readonly TipoMovimentacaoCaixa _tipo;

    [ObservableProperty] private string titulo;
    [ObservableProperty] private string valorTexto = string.Empty;
    [ObservableProperty] private string motivo = string.Empty;
    [ObservableProperty] private string? mensagemErro;

    public event Action? Salvo;

    public MovimentacaoCaixaWindowViewModel(ICaixaService caixaService, SessaoUsuario sessao, TipoMovimentacaoCaixa tipo)
    {
        _caixaService = caixaService;
        _sessao = sessao;
        _tipo = tipo;
        titulo = tipo == TipoMovimentacaoCaixa.Sangria ? "Sangria" : "Suprimento";
    }

    [RelayCommand]
    private async Task ConfirmarAsync()
    {
        if (!decimal.TryParse(ValorTexto, out var valor) || valor <= 0)
        {
            MensagemErro = "Informe um valor válido maior que zero.";
            return;
        }

        if (string.IsNullOrWhiteSpace(Motivo))
        {
            MensagemErro = "Informe o motivo.";
            return;
        }

        MensagemErro = null;

        try
        {
            await _caixaService.RegistrarMovimentacaoAsync(_sessao.UsuarioLogado!.Id, _tipo, valor, Motivo.Trim());
            Salvo?.Invoke();
        }
        catch (InvalidOperationException ex)
        {
            MensagemErro = ex.Message;
        }
    }
}
