using Vendex.Domain.Entities;
using Vendex.Domain.Enums;
using Vendex.Domain.Interfaces;

namespace Vendex.Application.Services;

public class UsuarioService : IUsuarioService
{
    private readonly IUnitOfWork _unitOfWork;

    public UsuarioService(IUnitOfWork unitOfWork)
    {
        _unitOfWork = unitOfWork;
    }

    public Task<IReadOnlyList<Usuario>> ListarAsync() => _unitOfWork.Usuarios.ObterTodosAsync();

    public async Task<bool> ExisteAlgumUsuarioAsync()
    {
        var usuarios = await _unitOfWork.Usuarios.ObterTodosAsync();
        return usuarios.Count > 0;
    }

    public async Task<Usuario> CriarUsuarioAsync(string nome, string login, string senha, TipoUsuario tipoUsuario)
    {
        var existente = await _unitOfWork.Usuarios.ObterPorLoginAsync(login);
        if (existente is not null)
            throw new InvalidOperationException($"Já existe um usuário com o login '{login}'.");

        var usuario = new Usuario
        {
            Nome = nome,
            Login = login,
            SenhaHash = BCrypt.Net.BCrypt.HashPassword(senha),
            TipoUsuario = tipoUsuario,
            Ativo = true,
            DataCadastro = DateTime.Now
        };

        await _unitOfWork.Usuarios.AdicionarAsync(usuario);
        await _unitOfWork.SalvarAlteracoesAsync();
        return usuario;
    }

    public async Task<Usuario?> ValidarLoginAsync(string login, string senha)
    {
        var usuario = await _unitOfWork.Usuarios.ObterPorLoginAsync(login);
        if (usuario is null || !usuario.Ativo)
            return null;

        return BCrypt.Net.BCrypt.Verify(senha, usuario.SenhaHash) ? usuario : null;
    }

    public async Task AtualizarAsync(int usuarioId, string nome, string login, TipoUsuario tipoUsuario, string? novaSenha)
    {
        var usuario = await _unitOfWork.Usuarios.ObterPorIdAsync(usuarioId);
        if (usuario is null)
            return;

        var existente = await _unitOfWork.Usuarios.ObterPorLoginAsync(login);
        if (existente is not null && existente.Id != usuarioId)
            throw new InvalidOperationException($"Já existe um usuário com o login '{login}'.");

        usuario.Nome = nome;
        usuario.Login = login;
        usuario.TipoUsuario = tipoUsuario;
        if (!string.IsNullOrEmpty(novaSenha))
            usuario.SenhaHash = BCrypt.Net.BCrypt.HashPassword(novaSenha);

        _unitOfWork.Usuarios.Atualizar(usuario);
        await _unitOfWork.SalvarAlteracoesAsync();
    }

    public async Task AlternarAtivoAsync(int usuarioId)
    {
        var usuario = await _unitOfWork.Usuarios.ObterPorIdAsync(usuarioId);
        if (usuario is null)
            return;

        usuario.Ativo = !usuario.Ativo;
        _unitOfWork.Usuarios.Atualizar(usuario);
        await _unitOfWork.SalvarAlteracoesAsync();
    }

    public Task<IReadOnlyList<Modulo>> ListarModulosAsync() => _unitOfWork.Modulos.ObterTodosAsync();

    public async Task<IReadOnlyList<string>> ObterNomesModulosPermitidosAsync(int usuarioId)
    {
        var permissoes = await _unitOfWork.UsuarioPermissoes.ObterTodosAsync();
        var modulos = await _unitOfWork.Modulos.ObterTodosAsync();

        var moduloIdsPermitidos = permissoes
            .Where(p => p.UsuarioId == usuarioId && p.PodeAcessar)
            .Select(p => p.ModuloId)
            .ToHashSet();

        return modulos
            .Where(m => moduloIdsPermitidos.Contains(m.Id))
            .Select(m => m.NomeModulo)
            .ToList();
    }

    public async Task DefinirPermissoesAsync(int usuarioId, IReadOnlyList<int> moduloIdsPermitidos)
    {
        var permissoesAtuais = await _unitOfWork.UsuarioPermissoes.ObterTodosAsync();
        foreach (var permissao in permissoesAtuais.Where(p => p.UsuarioId == usuarioId))
            _unitOfWork.UsuarioPermissoes.Remover(permissao);

        foreach (var moduloId in moduloIdsPermitidos)
        {
            await _unitOfWork.UsuarioPermissoes.AdicionarAsync(new UsuarioPermissao
            {
                UsuarioId = usuarioId,
                ModuloId = moduloId,
                PodeAcessar = true
            });
        }

        await _unitOfWork.SalvarAlteracoesAsync();
    }
}
