using CommunityToolkit.Mvvm.ComponentModel;

namespace Vendex.App.ViewModels;

public partial class ModuloPermissaoLinhaViewModel : ObservableObject
{
    public ModuloPermissaoLinhaViewModel(int id, string nomeModulo, bool podeVisualizar, bool podeCriar, bool podeEditar, bool podeExcluir)
    {
        Id = id;
        NomeModulo = nomeModulo;
        this.podeVisualizar = podeVisualizar;
        this.podeCriar = podeCriar;
        this.podeEditar = podeEditar;
        this.podeExcluir = podeExcluir;
    }

    public int Id { get; }
    public string NomeModulo { get; }

    [ObservableProperty] private bool podeVisualizar;
    [ObservableProperty] private bool podeCriar;
    [ObservableProperty] private bool podeEditar;
    [ObservableProperty] private bool podeExcluir;

    partial void OnPodeVisualizarChanged(bool value)
    {
        if (!value)
        {
            PodeCriar = false;
            PodeEditar = false;
            PodeExcluir = false;
        }
    }

    partial void OnPodeCriarChanged(bool value)
    {
        if (value) PodeVisualizar = true;
    }

    partial void OnPodeEditarChanged(bool value)
    {
        if (value) PodeVisualizar = true;
    }

    partial void OnPodeExcluirChanged(bool value)
    {
        if (value) PodeVisualizar = true;
    }
}
