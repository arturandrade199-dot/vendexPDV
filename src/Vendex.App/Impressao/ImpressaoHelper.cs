using System.Printing;
using System.Windows.Controls;
using System.Windows.Media;

namespace Vendex.App.Impressao;

/// <summary>Impressão usada pelos gatilhos automáticos (abertura/fechamento de caixa,
/// cupom de venda): se há uma impressora padrão salva em Configurações, imprime direto
/// nela sem abrir o diálogo do Windows. Sem impressora salva, cai no diálogo — os
/// botões manuais de "Imprimir" continuam com esse mesmo comportamento, sem usar esta
/// classe.</summary>
public static class ImpressaoHelper
{
    public static void Imprimir(Visual visual, string descricao, string? nomeImpressoraSalva)
    {
        var dialogoImpressao = new PrintDialog();
        if (!string.IsNullOrWhiteSpace(nomeImpressoraSalva))
        {
            dialogoImpressao.PrintQueue = new PrintQueue(new LocalPrintServer(), nomeImpressoraSalva);
            dialogoImpressao.PrintVisual(visual, descricao);
        }
        else if (dialogoImpressao.ShowDialog() == true)
        {
            dialogoImpressao.PrintVisual(visual, descricao);
        }
    }
}
