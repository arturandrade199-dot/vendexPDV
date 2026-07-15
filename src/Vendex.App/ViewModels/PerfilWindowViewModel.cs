using System.IO;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using Vendex.App.Navigation;
using Vendex.Application.Services;

namespace Vendex.App.ViewModels;

public partial class PerfilWindowViewModel : ObservableObject
{
    private readonly IUsuarioService _usuarioService;
    private readonly SessaoUsuario _sessao;
    private readonly int _usuarioId;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(IniciaisNome))]
    private string nome = string.Empty;

    [ObservableProperty] private string? caminhoFoto;
    [ObservableProperty] private string? mensagemErro;

    public string IniciaisNome => string.IsNullOrEmpty(Nome) ? "?" : Nome[..1].ToUpperInvariant();

    public event Action? Salvo;

    public PerfilWindowViewModel(IUsuarioService usuarioService, SessaoUsuario sessao)
    {
        _usuarioService = usuarioService;
        _sessao = sessao;

        var usuario = sessao.UsuarioLogado!;
        _usuarioId = usuario.Id;
        Nome = usuario.Nome;
        CaminhoFoto = usuario.FotoCaminho;
    }

    [RelayCommand]
    private void EscolherFoto()
    {
        var dialogo = new OpenFileDialog
        {
            Title = "Escolher foto de perfil",
            Filter = "Imagens (*.jpg;*.jpeg;*.png;*.bmp)|*.jpg;*.jpeg;*.png;*.bmp"
        };

        if (dialogo.ShowDialog() != true)
            return;

        Directory.CreateDirectory(AppPaths.PastaFotos);
        var destino = Path.Combine(AppPaths.PastaFotos, $"usuario_{_usuarioId}{Path.GetExtension(dialogo.FileName)}");
        File.Copy(dialogo.FileName, destino, overwrite: true);

        // Nome de arquivo igual não dispara o converter de novo — força a reavaliação do
        // binding trocando pra um caminho diferente e voltando pro definitivo.
        CaminhoFoto = null;
        CaminhoFoto = destino;
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        if (string.IsNullOrWhiteSpace(Nome))
        {
            MensagemErro = "Informe o nome.";
            return;
        }

        MensagemErro = null;
        var usuarioAtualizado = await _usuarioService.AtualizarPerfilAsync(_usuarioId, Nome.Trim(), CaminhoFoto);
        _sessao.UsuarioLogado = usuarioAtualizado;
        _sessao.NotificarUsuarioAtualizado();
        Salvo?.Invoke();
    }
}
