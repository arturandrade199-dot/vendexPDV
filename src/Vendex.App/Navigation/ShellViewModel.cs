using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Extensions.DependencyInjection;
using Vendex.App.ViewModels;

// PerfilWindow fica no namespace Vendex.App (raiz do projeto); ShellViewModel está em
// Vendex.App.Navigation, que não enxerga o namespace pai automaticamente em C#.
using Vendex.App;

namespace Vendex.App.Navigation;

/// <summary>
/// Estado da "moldura" do app (sidebar + cabeçalho) e implementação da navegação
/// entre módulos. Registrado como singleton — todo módulo que precisa navegar
/// (ex: MenuViewModel) recebe a mesma instância via INavigationService.
/// </summary>
public partial class ShellViewModel : ObservableObject, INavigationService
{
    private readonly IServiceProvider _serviceProvider;
    private readonly SessaoUsuario _sessao;
    private readonly Func<PerfilWindow> _perfilWindowFactory;
    private readonly Func<PdvWindow> _pdvWindowFactory;

    [ObservableProperty]
    private object? conteudoAtual;

    [ObservableProperty]
    [NotifyPropertyChangedFor(nameof(MostrarBotaoVoltar))]
    private string tituloAtual = "Menu";

    public bool MostrarBotaoVoltar => TituloAtual != "Menu";
    public bool EhAdministrador => _sessao.EhAdministrador;
    public string NomeUsuarioLogado => _sessao.UsuarioLogado?.Nome ?? string.Empty;
    public string IniciaisUsuarioLogado => string.IsNullOrEmpty(NomeUsuarioLogado) ? "?" : NomeUsuarioLogado[..1].ToUpperInvariant();
    public string? FotoUsuarioLogado => _sessao.UsuarioLogado?.FotoCaminho;
    public string NomeComPerfilUsuarioLogado => $"{NomeUsuarioLogado} ({(EhAdministrador ? "Administrador" : "Funcionário")})";

    public bool PodeAcessarContasPagar => _sessao.PodeAcessar("Contas a Pagar");
    public bool PodeAcessarProdutos => _sessao.PodeAcessar("Produtos");
    public bool PodeAcessarClientes => _sessao.PodeAcessar("Clientes");
    public bool PodeAcessarFornecedores => _sessao.PodeAcessar("Fornecedores");
    public bool PodeAcessarContasReceber => _sessao.PodeAcessar("Contas a Receber");
    public bool PodeAcessarPdv => _sessao.PodeAcessar("PDV");

    // Não navega para o Menu aqui: MenuViewModel precisa de INavigationService, que
    // aponta de volta para este singleton — resolvê-lo durante o próprio construtor
    // trava o container de DI (a instância ainda não terminou de ser criada). A
    // navegação inicial é disparada explicitamente pelo App.xaml.cs, depois que este
    // objeto já está totalmente construído.
    public ShellViewModel(IServiceProvider serviceProvider, SessaoUsuario sessao, Func<PerfilWindow> perfilWindowFactory, Func<PdvWindow> pdvWindowFactory)
    {
        _serviceProvider = serviceProvider;
        _sessao = sessao;
        _perfilWindowFactory = perfilWindowFactory;
        _pdvWindowFactory = pdvWindowFactory;

        // As propriedades de usuário logado (nome/iniciais/foto/tooltip) são calculadas a
        // partir de _sessao.UsuarioLogado — sem isso, editar o perfil não atualiza o
        // cabeçalho até reiniciar o app, porque ShellViewModel não notifica sozinho quando
        // o objeto por trás delas muda.
        _sessao.PropertyChanged += (_, e) =>
        {
            if (e.PropertyName != nameof(SessaoUsuario.UsuarioLogado))
                return;

            OnPropertyChanged(nameof(NomeUsuarioLogado));
            OnPropertyChanged(nameof(IniciaisUsuarioLogado));
            OnPropertyChanged(nameof(FotoUsuarioLogado));
            OnPropertyChanged(nameof(NomeComPerfilUsuarioLogado));
            OnPropertyChanged(nameof(EhAdministrador));
        };
    }

    [RelayCommand]
    private void AbrirPerfil() => _perfilWindowFactory().ShowDialog();

    public void NavegarPara<TViewModel>(string titulo) where TViewModel : notnull
    {
        ConteudoAtual = _serviceProvider.GetRequiredService<TViewModel>();
        TituloAtual = titulo;
    }

    [RelayCommand]
    private void IrParaMenu() => NavegarPara<MenuViewModel>("Menu");

    [RelayCommand]
    private void IrParaContasPagar() => NavegarPara<ContasPagarViewModel>("Contas a pagar");

    [RelayCommand]
    private void IrParaContasReceber() => NavegarPara<ContasReceberViewModel>("Contas a receber");

    [RelayCommand]
    private void IrParaProdutos() => NavegarPara<ProdutosViewModel>("Produtos");

    [RelayCommand]
    private void IrParaFornecedores() => NavegarPara<FornecedoresViewModel>("Fornecedores");

    [RelayCommand]
    private void IrParaClientes() => NavegarPara<ClientesViewModel>("Clientes");

    [RelayCommand]
    private void IrParaUsuarios() => NavegarPara<UsuariosViewModel>("Usuários");

    [RelayCommand]
    private void IrParaRelatorios() => NavegarPara<RelatoriosViewModel>("Relatórios");

    [RelayCommand]
    private void AbrirPdv() => _pdvWindowFactory().ShowDialog();
}
