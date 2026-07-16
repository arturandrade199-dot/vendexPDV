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

    public ReciboView()
    {
        InitializeComponent();
    }

    /// <summary>Chamado pelo fluxo de impressão automática (PDV, ver PdvWindow.xaml.cs) —
    /// imprime direto na impressora salva, sem abrir o diálogo do Windows. O botão manual
    /// "Imprimir cupom" (Imprimir_Click) continua sempre abrindo o diálogo.</summary>
    public void ImprimirAutomaticamente(string? nomeImpressoraSalva) =>
        ImpressaoHelper.Imprimir(ReciboParaImprimir, "Cupom Vendex", nomeImpressoraSalva);

    private void Imprimir_Click(object sender, System.Windows.RoutedEventArgs e)
    {
        var dialogoImpressao = new PrintDialog();
        if (dialogoImpressao.ShowDialog() == true)
        {
            dialogoImpressao.PrintVisual(ReciboParaImprimir, "Cupom Vendex");
        }
    }
}
