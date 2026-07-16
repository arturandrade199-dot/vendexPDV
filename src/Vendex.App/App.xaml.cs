using System.Globalization;
using System.IO;
using System.Windows;
using System.Windows.Markup;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Vendex.App.Navigation;
using Vendex.Application.Services;
using Vendex.Data;
using Vendex.Domain.Entities;
using Vendex.Domain.Interfaces;

namespace Vendex.App;

public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Sem isso, o WPF encerra o app sozinho assim que a LoginWindow fecha — ela é a
        // primeira (e, até a MainWindow abrir, única) janela, e o ShutdownMode padrão
        // (OnLastWindowClose) trata isso como "última janela fechada". Revertido para
        // OnMainWindowClose logo depois que a MainWindow é exibida com sucesso.
        ShutdownMode = ShutdownMode.OnExplicitShutdown;

        // Loja é sempre pt-BR (moeda, datas). Sem isso, `StringFormat={}{0:C2}` em XAML usa
        // "en-US" por padrão (propriedade FrameworkElement.Language), mesmo em Windows
        // configurado em português — resultado: preço aparecia como "$6.50" em vez de "R$ 6,50".
        var culturaBr = CultureInfo.GetCultureInfo("pt-BR");
        CultureInfo.DefaultThreadCurrentCulture = culturaBr;
        CultureInfo.DefaultThreadCurrentUICulture = culturaBr;
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culturaBr.IetfLanguageTag)));

        Directory.CreateDirectory(AppPaths.PastaDados);

        // Autodeclaração de licença exigida pelo QuestPDF antes de gerar qualquer PDF —
        // Community é gratuita para empresas com faturamento anual abaixo de US$1M, o caso
        // de uso do Vendex (ver decisão registrada com o usuário para o módulo de Relatórios).
        QuestPDF.Settings.License = QuestPDF.Infrastructure.LicenseType.Community;

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                // Singleton: a navegação entre módulos resolve ViewModels diretamente do
                // provider raiz (ShellViewModel), então o DbContext/UnitOfWork precisam
                // viver pela sessão inteira em vez de por escopo — app desktop de usuário
                // único, sem concorrência entre requisições como haveria numa API.
                services.AddDbContext<VendexDbContext>(options =>
                    options.UseSqlite($"Data Source={AppPaths.CaminhoBanco}"), ServiceLifetime.Singleton);

                services.AddSingleton<IUnitOfWork, UnitOfWork>();
                services.AddSingleton<IUsuarioService, UsuarioService>();
                services.AddSingleton<IAuditoriaService, AuditoriaService>();
                services.AddSingleton<IContaPagarService, ContaPagarService>();
                services.AddSingleton<IContaReceberService, ContaReceberService>();
                services.AddSingleton<ICaixaService, CaixaService>();
                services.AddSingleton<IProdutoService, ProdutoService>();
                services.AddSingleton<IVendaService, VendaService>();
                services.AddSingleton<IClienteService, ClienteService>();
                services.AddSingleton<IFornecedorService, FornecedorService>();
                services.AddSingleton<IBackupService, BackupService>();
                services.AddSingleton<AgendadorBackup>();
                services.AddSingleton<IRelatorioService, RelatorioService>();
                services.AddSingleton<IConfiguracaoImpressaoService, ConfiguracaoImpressaoService>();
                services.AddSingleton<ILicencaService, LicencaService>();
                services.AddSingleton<AgendadorLicenca>();

                services.AddSingleton<SessaoUsuario>();

                services.AddSingleton<ShellViewModel>();
                services.AddSingleton<INavigationService>(provedor => provedor.GetRequiredService<ShellViewModel>());

                services.AddTransient<ViewModels.MenuViewModel>();
                services.AddTransient<ViewModels.ContasPagarViewModel>();
                services.AddTransient<ViewModels.ContasReceberViewModel>();
                services.AddTransient<ViewModels.ProdutosViewModel>();
                services.AddTransient<ViewModels.FornecedoresViewModel>();
                services.AddTransient<ViewModels.ClientesViewModel>();
                services.AddTransient<ViewModels.UsuariosViewModel>();
                services.AddTransient<ViewModels.ConfiguracaoBackupViewModel>();
                services.AddTransient<ViewModels.RelatoriosViewModel>();
                services.AddTransient<ViewModels.VendasViewModel>();
                services.AddTransient<MainWindow>();

                services.AddTransient<Func<ViewModels.ReciboVenda, ReciboWindow>>(provedor => recibo => new ReciboWindow(recibo));

                services.AddTransient<ViewModels.LoginViewModel>();
                services.AddTransient<LoginWindow>();

                services.AddTransient<ViewModels.AtivacaoViewModel>();
                services.AddTransient<AtivacaoWindow>();

                services.AddTransient<ViewModels.PdvViewModel>();
                services.AddTransient<PdvWindow>();
                services.AddTransient<Func<PdvWindow>>(provedor => () => provedor.GetRequiredService<PdvWindow>());

                services.AddTransient<ViewModels.CaixaViewModel>();
                services.AddTransient<CaixaWindow>();
                services.AddTransient<Func<CaixaWindow>>(provedor => () => provedor.GetRequiredService<CaixaWindow>());

                services.AddTransient<ViewModels.PerfilWindowViewModel>();
                services.AddTransient<PerfilWindow>();
                services.AddTransient<Func<PerfilWindow>>(provedor => () => provedor.GetRequiredService<PerfilWindow>());

                services.AddTransient<Func<Domain.Enums.TipoMovimentacaoCaixa, MovimentacaoCaixaWindow>>(provedor => tipo =>
                {
                    var viewModel = new ViewModels.MovimentacaoCaixaWindowViewModel(
                        provedor.GetRequiredService<ICaixaService>(), provedor.GetRequiredService<SessaoUsuario>(), tipo);
                    return new MovimentacaoCaixaWindow(viewModel);
                });

                services.AddTransient<Func<IReadOnlyList<ViewModels.ItemCarrinhoViewModel>, ViewModels.FinalizarVendaViewModel>>(provedor => itens =>
                    new ViewModels.FinalizarVendaViewModel(
                        itens,
                        provedor.GetRequiredService<IVendaService>(),
                        provedor.GetRequiredService<SessaoUsuario>(),
                        provedor.GetRequiredService<IClienteService>()));

                services.AddTransient<ViewModels.NovaContaPagarViewModel>();
                services.AddTransient<NovaContaPagarWindow>();
                services.AddTransient<Func<NovaContaPagarWindow>>(provedor => () => provedor.GetRequiredService<NovaContaPagarWindow>());

                services.AddTransient<ViewModels.NovaContaReceberViewModel>();
                services.AddTransient<NovaContaReceberWindow>();
                services.AddTransient<Func<NovaContaReceberWindow>>(provedor => () => provedor.GetRequiredService<NovaContaReceberWindow>());

                services.AddTransient<Func<Produto?, ProdutoWindow>>(provedor => produtoExistente =>
                {
                    var viewModel = new ViewModels.ProdutoWindowViewModel(provedor.GetRequiredService<IProdutoService>(), produtoExistente);
                    return new ProdutoWindow(viewModel);
                });

                services.AddTransient<Func<Fornecedor?, FornecedorWindow>>(provedor => fornecedorExistente =>
                {
                    var viewModel = new ViewModels.FornecedorWindowViewModel(provedor.GetRequiredService<IFornecedorService>(), fornecedorExistente);
                    return new FornecedorWindow(viewModel);
                });

                services.AddTransient<Func<Cliente?, ClienteWindow>>(provedor => clienteExistente =>
                {
                    var viewModel = new ViewModels.ClienteWindowViewModel(provedor.GetRequiredService<IClienteService>(), clienteExistente);
                    return new ClienteWindow(viewModel);
                });

                services.AddTransient<Func<Usuario?, UsuarioWindow>>(provedor => usuarioExistente =>
                {
                    var viewModel = new ViewModels.UsuarioWindowViewModel(provedor.GetRequiredService<IUsuarioService>(), usuarioExistente);
                    return new UsuarioWindow(viewModel);
                });
            })
            .Build();

        var contexto = _host.Services.GetRequiredService<VendexDbContext>();
        contexto.Database.Migrate();
        SeedModulos(contexto);

        _host.Services.GetRequiredService<AgendadorBackup>().Iniciar();

        // Licenciamento por assinatura comentado por enquanto (backend do Supabase
        // ainda não configurado) — retomar depois de construir os próximos módulos.
        // Ver supabase/README.md pra reativar quando o backend estiver no ar.
        //
        // var licencaService = _host.Services.GetRequiredService<ILicencaService>();
        // var licencaLiberada = licencaService.VerificarELiberarAsync().GetAwaiter().GetResult();
        // if (!licencaLiberada)
        // {
        //     var ativacaoWindow = _host.Services.GetRequiredService<AtivacaoWindow>();
        //     if (ativacaoWindow.ShowDialog() != true)
        //     {
        //         Shutdown();
        //         return;
        //     }
        // }
        //
        // _host.Services.GetRequiredService<AgendadorLicenca>().Iniciar();

        var loginWindow = _host.Services.GetRequiredService<LoginWindow>();
        if (loginWindow.ShowDialog() != true)
        {
            Shutdown();
            return;
        }

        var shell = _host.Services.GetRequiredService<ShellViewModel>();
        shell.NavegarPara<ViewModels.MenuViewModel>("Menu");

        var janelaPrincipal = _host.Services.GetRequiredService<MainWindow>();

        // O WPF marca a primeira Window construída no processo (a LoginWindow, já
        // fechada nesse ponto) como Application.MainWindow e não atualiza sozinho — sem
        // isso, ConfigurarComoDialogo() das próximas janelas tentaria usar como Owner
        // uma janela de login já fechada.
        MainWindow = janelaPrincipal;
        janelaPrincipal.Show();
        ShutdownMode = ShutdownMode.OnMainWindowClose;
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        base.OnExit(e);
    }

    // Catálogo fixo de módulos para o controle de permissões por usuário (ver
    // arquitetura). Roda toda inicialização, mas só grava na primeira vez — sem
    // Migration.HasData porque essa lista pode crescer conforme novos módulos entram,
    // sem exigir uma nova migration a cada módulo novo.
    private static void SeedModulos(VendexDbContext contexto)
    {
        var nomes = new[] { "PDV", "Produtos", "Clientes", "Fornecedores", "Contas a Receber", "Contas a Pagar", "Caixa", "Vendas" };
        var existentes = contexto.Modulos.Select(m => m.NomeModulo).ToHashSet();
        var faltantes = nomes.Where(nome => !existentes.Contains(nome));

        contexto.Modulos.AddRange(faltantes.Select(nome => new Modulo { NomeModulo = nome }));
        contexto.SaveChanges();
    }
}
