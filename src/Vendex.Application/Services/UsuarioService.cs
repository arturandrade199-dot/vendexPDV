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
}
