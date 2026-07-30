using System.Windows.Input;
using Vendex.Application.Services;
using Vendex.App.Impressao;
using Vendex.App.ViewModels;
using Wpf.Ui.Controls;

namespace Vendex.App;

public partial class ReciboWindow : FluentWindow
{
    public static readonly RoutedCommand ImprimirCupomCommand = new();

    public ReciboWindow(ReciboVenda recibo, IConfiguracaoImpressaoService configuracaoImpressaoService)
    {
        InitializeComponent();
        this.ConfigurarComoDialogo();
        DataContext = recibo;

        CommandBindings.Add(new CommandBinding(ImprimirCupomCommand, (_, _) => ReciboControl.Imprimir()));

        // Fire-and-forget: a janela é modal (ShowDialog logo após o construtor), mas o
        // usuário ainda precisa ver a tela e clicar em "Imprimir" — tempo de sobra pra essa
        // leitura local no SQLite terminar antes de qualquer clique real.
        _ = ConfigurarLayoutImpressaoAsync(configuracaoImpressaoService);
    }

    private async Task ConfigurarLayoutImpressaoAsync(IConfiguracaoImpressaoService configuracaoImpressaoService)
    {
        var configuracao = await configuracaoImpressaoService.ObterConfiguracaoAsync();
        ReciboControl.UsarLayoutTermico = configuracao.UsarLayoutTermico;
        ReciboControl.DadosLoja = new DadosLojaImpressao(configuracao.NomeLoja, configuracao.EnderecoLoja, configuracao.CnpjLoja);
    }
}
