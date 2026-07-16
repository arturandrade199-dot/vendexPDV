using Vendex.App.ViewModels;
using Wpf.Ui.Controls;

namespace Vendex.App;

public partial class AtivacaoWindow : FluentWindow
{
    public AtivacaoWindow(AtivacaoViewModel viewModel)
    {
        InitializeComponent();
        this.ConfigurarComoDialogo();
        DataContext = viewModel;
        viewModel.Ativado += () =>
        {
            DialogResult = true;
            Close();
        };
    }
}
