namespace Vendex.Domain.Entities;

public class UsuarioPermissao : EntidadeBase
{
    public int UsuarioId { get; set; }
    public Usuario Usuario { get; set; } = null!;

    public int ModuloId { get; set; }
    public Modulo Modulo { get; set; } = null!;

    public bool PodeAcessar { get; set; }
}
