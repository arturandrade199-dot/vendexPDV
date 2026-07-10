namespace Vendex.Domain.Entities;

public class Modulo : EntidadeBase
{
    public string NomeModulo { get; set; } = string.Empty;

    public ICollection<UsuarioPermissao> Permissoes { get; set; } = new List<UsuarioPermissao>();
}
