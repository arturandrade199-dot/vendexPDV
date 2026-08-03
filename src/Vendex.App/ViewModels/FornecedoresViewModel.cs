using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.App.Navigation;
using Vendex.Application.Services;
using Vendex.Domain.Entities;

namespace Vendex.App.ViewModels;

public partial class FornecedoresViewModel : ObservableObject
{
    private const string NomeModulo = "Fornecedores";

    private readonly IFornecedorService _fornecedorService;
    private readonly Func<Fornecedor?, FornecedorWindow> _fornecedorWindowFactory;
    private readonly SessaoUsuario _sessao;
    private List<Fornecedor> _todosFornecedores = new();

    public ObservableCollection<FornecedorLinhaViewModel> Fornecedores { get; } = new();

    [ObservableProperty] private int totalFornecedores;
    [ObservableProperty] private FornecedorLinhaViewModel? itemSelecionado;
    [ObservableProperty] private string termoBusca = string.Empty;

    public bool PodeCriar => _sessao.PodeCriar(NomeModulo);
    public bool PodeEditar => _sessao.PodeEditar(NomeModulo);
    public bool PodeExcluir => _sessao.PodeExcluir(NomeModulo);

    public FornecedoresViewModel(IFornecedorService fornecedorService, Func<Fornecedor?, FornecedorWindow> fornecedorWindowFactory, SessaoUsuario sessao)
    {
        _fornecedorService = fornecedorService;
        _fornecedorWindowFactory = fornecedorWindowFactory;
        _sessao = sessao;
        _ = CarregarAsync();
    }

    [RelayCommand]
    private async Task AdicionarAsync()
    {
        if (!PodeCriar) return;

        var janela = _fornecedorWindowFactory(null);
        if (janela.ShowDialog() == true)
        {
            await CarregarAsync();
        }
    }

    [RelayCommand]
    private async Task EditarAsync(FornecedorLinhaViewModel? linha)
    {
        if (!PodeEditar || linha is null) return;

        var fornecedores = await _fornecedorService.ListarAsync();
        var alvo = fornecedores.FirstOrDefault(f => f.Id == linha.Id);
        if (alvo is null)
            return;

        var janela = _fornecedorWindowFactory(alvo);
        if (janela.ShowDialog() == true)
        {
            await CarregarAsync();
        }
    }

    [RelayCommand]
    private async Task RemoverAsync(FornecedorLinhaViewModel? linha)
    {
        if (!PodeExcluir || linha is null) return;

        var confirmar = System.Windows.MessageBox.Show(
            $"Remover o fornecedor \"{linha.Nome}\"?",
            "Confirmar remoção",
            System.Windows.MessageBoxButton.YesNo,
            System.Windows.MessageBoxImage.Question);

        if (confirmar != System.Windows.MessageBoxResult.Yes)
            return;

        await _fornecedorService.RemoverAsync(linha.Id);
        await CarregarAsync();
    }

    partial void OnTermoBuscaChanged(string value) => AplicarFiltro();

    private async Task CarregarAsync()
    {
        _todosFornecedores = (await _fornecedorService.ListarAsync()).OrderBy(f => f.Nome).ToList();
        TotalFornecedores = _todosFornecedores.Count;
        AplicarFiltro();
    }

    private void AplicarFiltro()
    {
        var termo = TermoBusca.Trim();
        var filtrados = string.IsNullOrEmpty(termo)
            ? _todosFornecedores
            : _todosFornecedores.Where(f =>
                f.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                (f.Telefone?.Contains(termo, StringComparison.OrdinalIgnoreCase) ?? false) ||
                (f.Documento?.Contains(termo, StringComparison.OrdinalIgnoreCase) ?? false));

        Fornecedores.Clear();
        foreach (var fornecedor in filtrados)
            Fornecedores.Add(new FornecedorLinhaViewModel(fornecedor));
    }
}
