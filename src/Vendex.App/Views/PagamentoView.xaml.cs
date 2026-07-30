using System.Windows.Controls;
using System.Windows.Threading;

namespace Vendex.App.Views;

public partial class PagamentoView : UserControl
{
    public PagamentoView()
    {
        InitializeComponent();

        // Nada dá foco a esta tela quando ela aparece (só troca Visibility do Border pai em
        // PdvWindow.xaml) — sem isso, os KeyBinding de F2/Enter daqui nunca veem o evento de
        // tecla, porque o foco continua em outro lugar (ex.: no próprio Window) e o KeyDown
        // nunca borbulha por essa subárvore. Adia pro layout terminar de aplicar o Visibility.
        DataContextChanged += (_, e) =>
        {
            if (e.NewValue is null)
                return;

            Dispatcher.BeginInvoke(new System.Action(() => TxtValor.Focus()), DispatcherPriority.ContextIdle);
        };
    }
}
