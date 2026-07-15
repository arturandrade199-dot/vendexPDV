using System.Windows.Media;
using Vendex.Domain.Entities;
using Vendex.Domain.Enums;

namespace Vendex.App.ViewModels;

public class UsuarioLinhaViewModel
{
    public UsuarioLinhaViewModel(Usuario usuario)
    {
        Id = usuario.Id;
        Nome = usuario.Nome;
        Login = usuario.Login;
        TipoUsuarioTexto = usuario.TipoUsuario == TipoUsuario.Administrador ? "Administrador" : "Funcionário";
        DataCadastroFormatada = usuario.DataCadastro.ToString("dd/MM/yyyy");
        Ativo = usuario.Ativo;

        (SituacaoTexto, SituacaoFundo, SituacaoCor) = usuario.Ativo
            ? ("Ativo", Color.FromRgb(0xDC, 0xF5, 0xE3), Color.FromRgb(0x1B, 0x8A, 0x4B))
            : ("Inativo", Color.FromRgb(0xF1, 0xF2, 0xF4), Color.FromRgb(0x6B, 0x72, 0x80));
    }

    public int Id { get; }
    public string Nome { get; }
    public string Login { get; }
    public string TipoUsuarioTexto { get; }
    public string DataCadastroFormatada { get; }
    public bool Ativo { get; }
    public string SituacaoTexto { get; }
    public Color SituacaoFundo { get; }
    public Color SituacaoCor { get; }
    public string RotuloAlternarAtivo => Ativo ? "Desativar" : "Ativar";
}
