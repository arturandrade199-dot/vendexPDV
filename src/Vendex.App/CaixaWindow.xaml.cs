using System.Windows.Controls;
using System.Windows.Input;
using Vendex.App.Impressao;
using Vendex.App.ViewModels;
using Vendex.Application.Services;
using Wpf.Ui.Controls;

namespace Vendex.App;

public partial class CaixaWindow : FluentWindow
{
    // RoutedCommand porque o atalho (Ctrl+P) precisa decidir, na hora, qual dos dois recibos
    // (abertura ou fechamento) imprimir — depende do estado atual do CaixaViewModel.
    public static readonly RoutedCommand ImprimirCommand = new();

    private readonly CaixaViewModel _viewModel;

    public CaixaWindow(CaixaViewModel viewModel, IConfiguracaoImpressaoService configuracaoImpressaoService)
    {
        InitializeComponent();
        this.ConfigurarComoDialogo();
        _viewModel = viewModel;
        DataContext = viewModel;

        CommandBindings.Add(new CommandBinding(ImprimirCommand, (_, _) => ImprimirReciboAtual()));

        viewModel.Concluido += () =>
        {
            DialogResult = true;
            Close();
        };

        viewModel.PropertyChanged += async (_, e) =>
        {
            if (e.PropertyName != nameof(CaixaViewModel.Estado))
                return;

            if (viewModel.Estado == CaixaViewModel.EstadoTela.AbrirRecibo ||
                viewModel.Estado == CaixaViewModel.EstadoTela.FecharRecibo)
            {
                var configuracao = await configuracaoImpressaoService.ObterConfiguracaoAsync();

                if (viewModel.Estado == CaixaViewModel.EstadoTela.AbrirRecibo && configuracao.ImprimirAberturaCaixa)
                    ImpressaoHelper.Imprimir(ReciboAberturaParaImprimir, "Abertura de Caixa", configuracao.ImpressoraPadrao);
                else if (viewModel.Estado == CaixaViewModel.EstadoTela.FecharRecibo && configuracao.ImprimirFechamentoCaixa)
                    ImpressaoHelper.Imprimir(ReciboFechamentoParaImprimir, "Fechamento de Caixa", configuracao.ImpressoraPadrao);
            }
        };
    }

    private void ImprimirAbertura_Click(object sender, System.Windows.RoutedEventArgs e) =>
        Imprimir(ReciboAberturaParaImprimir, "Abertura de Caixa");

    private void ImprimirFechamento_Click(object sender, System.Windows.RoutedEventArgs e) =>
        Imprimir(ReciboFechamentoParaImprimir, "Fechamento de Caixa");

    private void ImprimirReciboAtual()
    {
        if (_viewModel.Estado == CaixaViewModel.EstadoTela.AbrirRecibo)
            Imprimir(ReciboAberturaParaImprimir, "Abertura de Caixa");
        else if (_viewModel.Estado == CaixaViewModel.EstadoTela.FecharRecibo)
            Imprimir(ReciboFechamentoParaImprimir, "Fechamento de Caixa");
    }

    private static void Imprimir(System.Windows.UIElement visual, string nomeDocumento)
    {
        var dialogoImpressao = new PrintDialog();
        if (dialogoImpressao.ShowDialog() == true)
        {
            dialogoImpressao.PrintVisual(visual, nomeDocumento);
        }
    }
}
