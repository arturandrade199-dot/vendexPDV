using System.Windows.Input;
using Wpf.Ui.Controls;

namespace Vendex.App.ViewModels;

public class ModuloTile
{
    public ModuloTile(string nome, string descricao, SymbolRegular icone, bool disponivel, ICommand? comando)
    {
        Nome = nome;
        Descricao = descricao;
        Icone = icone;
        Disponivel = disponivel;
        Comando = comando;
        RotuloDisponibilidade = disponivel ? string.Empty : "Em breve";
    }

    public string Nome { get; }
    public string Descricao { get; }
    public SymbolRegular Icone { get; }
    public bool Disponivel { get; }
    public ICommand? Comando { get; }
    public string RotuloDisponibilidade { get; }
}
