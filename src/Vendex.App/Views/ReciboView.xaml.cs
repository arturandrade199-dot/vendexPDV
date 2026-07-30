using System.Printing;
using System.Windows;
using System.Windows.Controls;
using Vendex.App.Impressao;

namespace Vendex.App.Views;

public partial class ReciboView : UserControl
{
    /// <summary>Controla se o botão "Nova Venda" aparece — ele assume que o DataContext
    /// ancestral é o PdvViewModel (de onde vem NovaVendaCommand), o que não é verdade
    /// quando este controle é reaproveitado só pra visualizar/reimprimir uma venda
    /// antiga (ver ReciboWindow, usado pelo módulo Vendas).</summary>
    public bool PermitirNovaVenda { get; set; } = true;

    /// <summary>Configurados pela Window hospedeira (PdvWindow/ReciboWindow) a partir de
    /// ConfiguracaoImpressao antes de qualquer impressão — inclusive antes da automática,
    /// já que o clique manual em "Imprimir cupom" pode acontecer bem antes disso.</summary>
    public bool UsarLayoutTermico { get; set; }
    public DadosLojaImpressao? DadosLoja { get; set; }

    public ReciboView()
    {
        InitializeComponent();
    }

    /// <summary>Chamado pelo fluxo de impressão automática (PDV, ver PdvWindow.xaml.cs) —
    /// imprime direto na impressora salva, sem abrir o diálogo do Windows. O botão manual
    /// "Imprimir cupom" (Imprimir_Click) continua sempre abrindo o diálogo.</summary>
    public void ImprimirAutomaticamente(string? nomeImpressoraSalva)
    {
        var larguraImprimivel = ObterLarguraImprimivel(nomeImpressoraSalva);
        ImpressaoHelper.Imprimir(CriarVisualImpressao(larguraImprimivel), "Cupom Vendex", nomeImpressoraSalva);
    }

    public void Imprimir()
    {
        var dialogoImpressao = new PrintDialog();
        if (dialogoImpressao.ShowDialog() == true)
        {
            dialogoImpressao.PrintVisual(CriarVisualImpressao(dialogoImpressao.PrintableAreaWidth), "Cupom Vendex");
        }
    }

    /// <summary>PrintDialog novo só pra descobrir a área imprimível da impressora salva antes
    /// de montar o visual — precisa ser a mesma PrintQueue que o ImpressaoHelper vai usar de
    /// verdade pra imprimir, senão a largura calculada aqui não bate com a impressora real.</summary>
    private static double ObterLarguraImprimivel(string? nomeImpressoraSalva)
    {
        var dialogoImpressao = new PrintDialog();
        if (!string.IsNullOrWhiteSpace(nomeImpressoraSalva))
            dialogoImpressao.PrintQueue = new PrintQueue(new LocalPrintServer(), nomeImpressoraSalva);

        return dialogoImpressao.PrintableAreaWidth;
    }

    /// <summary>O layout moderno (ReciboParaImprimir) já está na árvore visual, medido e
    /// arranjado pelo próprio layout da tela. O térmico não — é criado do zero só pra
    /// imprimir, então precisa de Measure/Arrange manual antes do PrintVisual conseguir
    /// rasterizá-lo (senão sai com tamanho zero). A largura nunca passa da área imprimível
    /// real da impressora — passar disso corta o conteúdo à direita (foi o que aconteceu
    /// com 302px fixo numa impressora com área útil menor).</summary>
    private UIElement CriarVisualImpressao(double larguraImprimivel)
    {
        if (!UsarLayoutTermico)
            return ReciboParaImprimir;

        var recibo = (ViewModels.ReciboVenda)DataContext;
        var largura = larguraImprimivel > 0 ? Math.Min(302, larguraImprimivel) : 302;
        var visual = new ReciboTermicoView { DataContext = new ReciboTermicoDados(recibo, DadosLoja), Width = largura };

        visual.Measure(new Size(largura, double.PositiveInfinity));
        visual.Arrange(new Rect(0, 0, largura, visual.DesiredSize.Height));
        return visual;
    }

    private void Imprimir_Click(object sender, System.Windows.RoutedEventArgs e) => Imprimir();
}
