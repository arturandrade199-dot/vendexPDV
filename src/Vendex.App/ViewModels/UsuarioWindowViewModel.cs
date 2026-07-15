using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.Application.Services;
using Vendex.Domain.Entities;
using Vendex.Domain.Enums;

namespace Vendex.App.ViewModels;

public partial class UsuarioWindowViewModel : ObservableObject
{
    private readonly IUsuarioService _usuarioService;
    private readonly int? _usuarioId;

    [ObservableProperty] private string titulo = "Novo usuário";
    [ObservableProperty] private string nome = string.Empty;
    [ObservableProperty] private string login = string.Empty;
    [ObservableProperty] private string senha = string.Empty;
    [ObservableProperty] private string rotuloSenha = "Senha";
    [ObservableProperty] private bool ehAdministrador;
    [ObservableProperty] private string? mensagemErro;

    public ObservableCollection<ModuloPermissaoLinhaViewModel> ModulosDisponiveis { get; } = new();

    public event Action? Salvo;

    public UsuarioWindowViewModel(IUsuarioService usuarioService, Usuario? usuarioExistente)
    {
        _usuarioService = usuarioService;

        if (usuarioExistente is not null)
        {
            _usuarioId = usuarioExistente.Id;
            Titulo = "Editar usuário";
            Nome = usuarioExistente.Nome;
            Login = usuarioExistente.Login;
            EhAdministrador = usuarioExistente.TipoUsuario == TipoUsuario.Administrador;
            RotuloSenha = "Nova senha (deixe em branco para manter a atual)";
        }

        _ = CarregarModulosAsync();
    }

    private async Task CarregarModulosAsync()
    {
        if (_usuarioId is int id)
        {
            var permissoes = await _usuarioService.ObterPermissoesModulosAsync(id);
            ModulosDisponiveis.Clear();
            foreach (var permissao in permissoes)
                ModulosDisponiveis.Add(new ModuloPermissaoLinhaViewModel(permissao.ModuloId, permissao.NomeModulo,
                    permissao.PodeVisualizar, permissao.PodeCriar, permissao.PodeEditar, permissao.PodeExcluir));
        }
        else
        {
            var modulos = await _usuarioService.ListarModulosAsync();
            ModulosDisponiveis.Clear();
            foreach (var modulo in modulos)
                ModulosDisponiveis.Add(new ModuloPermissaoLinhaViewModel(modulo.Id, modulo.NomeModulo, false, false, false, false));
        }
    }

    [RelayCommand]
    private async Task SalvarAsync()
    {
        if (string.IsNullOrWhiteSpace(Nome) || string.IsNullOrWhiteSpace(Login))
        {
            MensagemErro = "Informe nome e login.";
            return;
        }

        if (_usuarioId is null && string.IsNullOrWhiteSpace(Senha))
        {
            MensagemErro = "Informe a senha.";
            return;
        }

        if (!string.IsNullOrEmpty(Senha) && Senha.Length < 4)
        {
            MensagemErro = "A senha deve ter ao menos 4 caracteres.";
            return;
        }

        var tipo = EhAdministrador ? TipoUsuario.Administrador : TipoUsuario.Funcionario;
        var permissoes = ModulosDisponiveis
            .Select(m => new PermissaoModulo(m.Id, m.NomeModulo, m.PodeVisualizar, m.PodeCriar, m.PodeEditar, m.PodeExcluir))
            .ToList();

        try
        {
            int usuarioId;
            if (_usuarioId is int id)
            {
                await _usuarioService.AtualizarAsync(id, Nome.Trim(), Login.Trim(), tipo, string.IsNullOrEmpty(Senha) ? null : Senha);
                usuarioId = id;
            }
            else
            {
                var novoUsuario = await _usuarioService.CriarUsuarioAsync(Nome.Trim(), Login.Trim(), Senha, tipo);
                usuarioId = novoUsuario.Id;
            }

            await _usuarioService.DefinirPermissoesAsync(usuarioId, permissoes);

            MensagemErro = null;
            Salvo?.Invoke();
        }
        catch (InvalidOperationException ex)
        {
            MensagemErro = ex.Message;
        }
    }
}
