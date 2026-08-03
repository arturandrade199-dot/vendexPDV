using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.Domain.Entities;

namespace Vendex.App.ViewModels;

/// <summary>Diálogo de busca de cliente por nome/telefone — abre a partir de uma lista já
/// carregada pela tela chamadora (sem ida ao banco de novo) porque um ComboBox simples fica
/// inviável de usar com muitos clientes cadastrados.</summary>
public partial class SelecionarClienteWindowViewModel : ObservableObject
{
    private readonly List<Cliente> _todos;

    public ObservableCollection<Cliente> Resultados { get; } = new();

    [ObservableProperty] private string termoBusca = string.Empty;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(PodeConfirmar))]
    private Cliente? clienteSelecionado;

    public bool PodeConfirmar => ClienteSelecionado is not null;

    public event Action? Confirmado;

    public SelecionarClienteWindowViewModel(IReadOnlyList<Cliente> clientes)
    {
        _todos = clientes.ToList();
        AplicarFiltro();
    }

    partial void OnTermoBuscaChanged(string value) => AplicarFiltro();

    private void AplicarFiltro()
    {
        var termo = TermoBusca.Trim();
        var filtrados = string.IsNullOrEmpty(termo)
            ? _todos
            : _todos.Where(c =>
                c.Nome.Contains(termo, StringComparison.OrdinalIgnoreCase) ||
                (c.Telefone?.Contains(termo, StringComparison.OrdinalIgnoreCase) ?? false));

        Resultados.Clear();
        foreach (var cliente in filtrados.OrderBy(c => c.Nome))
            Resultados.Add(cliente);
    }

    [RelayCommand]
    private void Confirmar()
    {
        if (ClienteSelecionado is not null)
            Confirmado?.Invoke();
    }
}
