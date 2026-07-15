using System.Windows;
using System.Windows.Input;

namespace Vendex.App;

internal static class WindowExtensions
{
    /// <summary>
    /// Aplica o comportamento padrão das janelas de cadastro/diálogo: centralizar sobre a
    /// janela principal (CenterOwner só funciona com Owner definido — Window não faz isso
    /// sozinho) e fechar com Esc, de qualquer controle focado (PreviewKeyDown tuneliza a
    /// partir da própria janela antes de chegar no controle filho).
    /// </summary>
    public static void ConfigurarComoDialogo(this Window janela)
    {
        // O WPF define Application.MainWindow automaticamente para a primeira Window
        // construída no processo (ex: a própria LoginWindow, na primeira execução) —
        // sem essa checagem, essas janelas tentariam virar Owner de si mesmas.
        var janelaPrincipal = System.Windows.Application.Current.MainWindow;
        if (janelaPrincipal is not null && !ReferenceEquals(janelaPrincipal, janela))
            janela.Owner = janelaPrincipal;

        janela.PreviewKeyDown += (_, e) =>
        {
            if (e.Key == Key.Escape)
                janela.Close();
        };
    }
}
