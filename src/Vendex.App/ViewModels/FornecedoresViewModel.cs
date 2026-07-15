using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.Application.Services;
using Vendex.Domain.Entities;

namespace Vendex.App.ViewModels;

public partial class FornecedoresViewModel : ObservableObject
{
    private readonly IFornecedorService _fornecedorService;
    private readonly Func<Fornecedor?, FornecedorWindow> _fornecedorWindowFactory;

    public ObservableCollection<FornecedorLinhaViewModel> Fornecedores { get; } = new();

    [ObservableProperty] private int totalFornecedores;

    public FornecedoresViewModel(IFornecedorService fornecedorService, Func<Fornecedor?, FornecedorWindow> fornecedorWindowFactory)
    {
        _fornecedorService = fornecedorService;
        _fornecedorWindowFactory = fornecedorWindowFactory;
        _ = CarregarAsync();
    }

    [RelayCommand]
    private async Task AdicionarAsync()
    {
        var janela = _fornecedorWindowFactory(null);
        if (janela.ShowDialog() == true)
        {
            await CarregarAsync();
        }
    }

    [RelayCommand]
    private async Task EditarAsync(FornecedorLinhaViewModel linha)
    {
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
    private async Task RemoverAsync(FornecedorLinhaViewModel linha)
    {
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

    private async Task CarregarAsync()
    {
        var fornecedores = await _fornecedorService.ListarAsync();
        Fornecedores.Clear();
        foreach (var fornecedor in fornecedores.OrderBy(f => f.Nome))
            Fornecedores.Add(new FornecedorLinhaViewModel(fornecedor));

        TotalFornecedores = fornecedores.Count;
    }
}
