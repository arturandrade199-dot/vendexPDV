using Vendex.Domain.Enums;

namespace Vendex.Domain.Entities;

public class Usuario : EntidadeBase
{
    public string Nome { get; set; } = string.Empty;
    public string Login { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public TipoUsuario TipoUsuario { get; set; }
    public bool Ativo { get; set; } = true;
    public DateTime DataCadastro { get; set; } = DateTime.Now;

    public ICollection<UsuarioPermissao> Permissoes { get; set; } = new List<UsuarioPermissao>();
}
