using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.Application.Services;
using Vendex.Domain.Entities;

namespace Vendex.App.ViewModels;

public partial class UsuariosViewModel : ObservableObject
{
    private readonly IUsuarioService _usuarioService;
    private readonly Func<Usuario?, UsuarioWindow> _usuarioWindowFactory;

    public ObservableCollection<UsuarioLinhaViewModel> Usuarios { get; } = new();

    [ObservableProperty] private int totalUsuarios;

    public UsuariosViewModel(IUsuarioService usuarioService, Func<Usuario?, UsuarioWindow> usuarioWindowFactory)
    {
        _usuarioService = usuarioService;
        _usuarioWindowFactory = usuarioWindowFactory;
        _ = CarregarAsync();
    }

    [RelayCommand]
    private async Task AdicionarAsync()
    {
        var janela = _usuarioWindowFactory(null);
        if (janela.ShowDialog() == true)
        {
            await CarregarAsync();
        }
    }

    [RelayCommand]
    private async Task EditarAsync(UsuarioLinhaViewModel linha)
    {
        var usuarios = await _usuarioService.ListarAsync();
        var alvo = usuarios.FirstOrDefault(u => u.Id == linha.Id);
        if (alvo is null)
            return;

        var janela = _usuarioWindowFactory(alvo);
        if (janela.ShowDialog() == true)
        {
            await CarregarAsync();
        }
    }

    [RelayCommand]
    private async Task AlternarAtivoAsync(UsuarioLinhaViewModel linha)
    {
        await _usuarioService.AlternarAtivoAsync(linha.Id);
        await CarregarAsync();
    }

    private async Task CarregarAsync()
    {
        var usuarios = await _usuarioService.ListarAsync();
        Usuarios.Clear();
        foreach (var usuario in usuarios.OrderBy(u => u.Nome))
            Usuarios.Add(new UsuarioLinhaViewModel(usuario));

        TotalUsuarios = usuarios.Count;
    }
}
