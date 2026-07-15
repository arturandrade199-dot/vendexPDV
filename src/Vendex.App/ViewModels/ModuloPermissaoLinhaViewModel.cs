using CommunityToolkit.Mvvm.ComponentModel;

namespace Vendex.App.ViewModels;

public partial class ModuloPermissaoLinhaViewModel : ObservableObject
{
    public ModuloPermissaoLinhaViewModel(int id, string nomeModulo, bool permitido)
    {
        Id = id;
        NomeModulo = nomeModulo;
        Permitido = permitido;
    }

    public int Id { get; }
    public string NomeModulo { get; }

    [ObservableProperty] private bool permitido;
}
