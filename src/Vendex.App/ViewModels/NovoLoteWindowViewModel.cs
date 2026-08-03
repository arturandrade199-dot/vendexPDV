using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.Application.Services;

namespace Vendex.App.ViewModels;

public partial class NovoLoteWindowViewModel : ObservableObject
{
    private readonly ILoteService _loteService;
    private readonly Func<SelecionarProdutoWindow> _selecionarProdutoWindowFactory;

    [ObservableProperty] private ResultadoBuscaProduto? produtoSelecionado;
    [ObservableProperty] private DateTime? dataFabricacao = DateTime.Today;
    [ObservableProperty] private DateTime? dataValidade;
    [ObservableProperty] private string quantidadeTexto = string.Empty;
    [ObservableProperty] private string observacoes = string.Empty;
    [ObservableProperty] private string? mensagemErro;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PodeConfirmar))]
    private bool salvando;

    public bool PodeConfirmar => !Salvando;

    public event Action? Salvo;

    public NovoLoteWindowViewModel(ILoteService loteService, Func<SelecionarProdutoWindow> selecionarProdutoWindowFactory)
    {
        _loteService = loteService;
        _selecionarProdutoWindowFactory = selecionarProdutoWindowFactory;
    }

    [RelayCommand]
    private void AbrirSelecaoProduto()
    {
        var janela = _selecionarProdutoWindowFactory();
        if (janela.ShowDialog() == true)
            ProdutoSelecionado = janela.ProdutoSelecionado;
    }

    [RelayCommand]
    private async Task ConfirmarAsync()
    {
        if (ProdutoSelecionado is null)
        {
            MensagemErro = "Selecione o produto.";
            return;
        }

        if (DataFabricacao is null || DataValidade is null)
        {
            MensagemErro = "Informe a data de fabricação/entrada e a data de validade.";
            return;
        }

        if (!decimal.TryParse(QuantidadeTexto, out var quantidade) || quantidade <= 0)
        {
            MensagemErro = "Informe uma quantidade válida.";
            return;
        }

        MensagemErro = null;
        Salvando = true;
        try
        {
            await _loteService.RegistrarAsync(
                ProdutoSelecionado.ProdutoId, ProdutoSelecionado.VarianteId, DataFabricacao.Value, DataValidade.Value,
                quantidade, string.IsNullOrWhiteSpace(Observacoes) ? null : Observacoes.Trim());

            Salvo?.Invoke();
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
}
