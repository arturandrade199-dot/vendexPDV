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
using Vendex.Domain.Enums;
using Vendex.Domain.Interfaces;

namespace Vendex.App;

public partial class App : System.Windows.Application
{
    private IHost? _host;

    protected override void OnStartup(StartupEventArgs e)
    {
        base.OnStartup(e);

        // Loja é sempre pt-BR (moeda, datas). Sem isso, `StringFormat={}{0:C2}` em XAML usa
        // "en-US" por padrão (propriedade FrameworkElement.Language), mesmo em Windows
        // configurado em português — resultado: preço aparecia como "$6.50" em vez de "R$ 6,50".
        var culturaBr = CultureInfo.GetCultureInfo("pt-BR");
        CultureInfo.DefaultThreadCurrentCulture = culturaBr;
        CultureInfo.DefaultThreadCurrentUICulture = culturaBr;
        FrameworkElement.LanguageProperty.OverrideMetadata(
            typeof(FrameworkElement),
            new FrameworkPropertyMetadata(XmlLanguage.GetLanguage(culturaBr.IetfLanguageTag)));

        var pastaDados = Path.Combine(AppContext.BaseDirectory, "dados");
        Directory.CreateDirectory(pastaDados);
        var caminhoBanco = Path.Combine(pastaDados, "vendex.db");

        _host = Host.CreateDefaultBuilder()
            .ConfigureServices((_, services) =>
            {
                // Singleton: a navegação entre módulos resolve ViewModels diretamente do
                // provider raiz (ShellViewModel), então o DbContext/UnitOfWork precisam
                // viver pela sessão inteira em vez de por escopo — app desktop de usuário
                // único, sem concorrência entre requisições como haveria numa API.
                services.AddDbContext<VendexDbContext>(options =>
                    options.UseSqlite($"Data Source={caminhoBanco}"), ServiceLifetime.Singleton);

                services.AddSingleton<IUnitOfWork, UnitOfWork>();
                services.AddSingleton<IUsuarioService, UsuarioService>();
                services.AddSingleton<IAuditoriaService, AuditoriaService>();
                services.AddSingleton<IContaPagarService, ContaPagarService>();
                services.AddSingleton<IProdutoService, ProdutoService>();
                services.AddSingleton<IVendaService, VendaService>();
                services.AddSingleton<IClienteService, ClienteService>();

                services.AddSingleton<ShellViewModel>();
                services.AddSingleton<INavigationService>(provedor => provedor.GetRequiredService<ShellViewModel>());

                services.AddTransient<ViewModels.MenuViewModel>();
                services.AddTransient<ViewModels.ContasPagarViewModel>();
                services.AddTransient<ViewModels.ProdutosViewModel>();
                services.AddTransient<MainWindow>();

                services.AddTransient<ViewModels.PdvViewModel>();
                services.AddTransient<PdvWindow>();
                services.AddTransient<Func<PdvWindow>>(provedor => () => provedor.GetRequiredService<PdvWindow>());

                services.AddTransient<Func<IReadOnlyList<ViewModels.ItemCarrinhoViewModel>, ViewModels.FinalizarVendaViewModel>>(provedor => itens =>
                    new ViewModels.FinalizarVendaViewModel(
                        itens,
                        provedor.GetRequiredService<IVendaService>(),
                        provedor.GetRequiredService<IUsuarioService>(),
                        provedor.GetRequiredService<IClienteService>()));

                services.AddTransient<ViewModels.NovaContaPagarViewModel>();
                services.AddTransient<NovaContaPagarWindow>();
                services.AddTransient<Func<NovaContaPagarWindow>>(provedor => () => provedor.GetRequiredService<NovaContaPagarWindow>());

                services.AddTransient<Func<Produto?, ProdutoWindow>>(provedor => produtoExistente =>
                {
                    var viewModel = new ViewModels.ProdutoWindowViewModel(provedor.GetRequiredService<IProdutoService>(), produtoExistente);
                    return new ProdutoWindow(viewModel);
                });
            })
            .Build();

        var contexto = _host.Services.GetRequiredService<VendexDbContext>();
        contexto.Database.Migrate();

        // Ainda não existe tela de Ativação/Login (ver arquitetura). Enquanto isso, garante
        // um usuário Administrador padrão para satisfazer o FK obrigatório de Venda.UsuarioId
        // — remover assim que o módulo de Login existir e passar a criar o usuário de verdade.
        var usuarioService = _host.Services.GetRequiredService<IUsuarioService>();
        if (!usuarioService.ExisteAlgumUsuarioAsync().GetAwaiter().GetResult())
        {
            usuarioService.CriarUsuarioAsync("Administrador", "admin", "admin123", TipoUsuario.Administrador)
                .GetAwaiter().GetResult();
        }

        var shell = _host.Services.GetRequiredService<ShellViewModel>();
        shell.NavegarPara<ViewModels.MenuViewModel>("Menu");

        var janelaPrincipal = _host.Services.GetRequiredService<MainWindow>();
        janelaPrincipal.Show();
    }

    protected override void OnExit(ExitEventArgs e)
    {
        _host?.Dispose();
        base.OnExit(e);
    }
}
