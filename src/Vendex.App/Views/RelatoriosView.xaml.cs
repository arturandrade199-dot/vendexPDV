using System.ComponentModel;
using System.Windows.Controls;
using System.Windows.Data;
using Vendex.App.ViewModels;

namespace Vendex.App.Views;

public partial class RelatoriosView : UserControl
{
    public RelatoriosView()
    {
        InitializeComponent();
        DataContextChanged += (_, e) =>
        {
            if (e.OldValue is RelatoriosViewModel antigo)
                antigo.PropertyChanged -= ViewModel_PropertyChanged;
            if (e.NewValue is RelatoriosViewModel novo)
            {
                novo.PropertyChanged += ViewModel_PropertyChanged;
                AtualizarColunas(novo.Resultado);
            }
        };
    }

    private void ViewModel_PropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(RelatoriosViewModel.Resultado) && sender is RelatoriosViewModel viewModel)
            AtualizarColunas(viewModel.Resultado);
    }

    // WPF não tem binding declarativo pra colunas dinâmicas de ListView/DataGrid — o
    // resultado de cada um dos 10 relatórios tem um número/nome de colunas diferente,
    // então elas são reconstruídas em código sempre que o relatório gerado muda.
    private void AtualizarColunas(Vendex.Application.Services.RelatorioResultado? resultado)
    {
        var gridView = new GridView();
        if (resultado is not null)
        {
            for (var indice = 0; indice < resultado.Colunas.Count; indice++)
            {
                gridView.Columns.Add(new GridViewColumn
                {
                    Header = resultado.Colunas[indice].Titulo,
                    DisplayMemberBinding = new Binding($"[{indice}]"),
                    Width = Math.Max(100, 400 / resultado.Colunas.Count)
                });
            }
        }

        ListaResultado.View = gridView;
    }
}
