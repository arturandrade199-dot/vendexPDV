using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.App.Navigation;
using Wpf.Ui.Controls;

namespace Vendex.App.ViewModels;

public class MenuViewModel : ObservableObject
{
    public ObservableCollection<ModuloTile> Modulos { get; }

    public MenuViewModel(INavigationService navegacao, Func<PdvWindow> pdvWindowFactory, Func<CaixaWindow> caixaWindowFactory,
        Func<DevolucaoWindow> devolucaoWindowFactory, Func<string, string, AutorizacaoWindow> autorizacaoWindowFactory, SessaoUsuario sessao)
    {
        var modulos = new List<ModuloTile>();

        if (sessao.PodeAcessar("PDV"))
        {
            modulos.Add(new ModuloTile("PDV (Venda)", "Registrar vendas no balcão", SymbolRegular.Cart24, true,
                new RelayCommand(() => pdvWindowFactory().ShowDialog())));

            // Mesma trava de permissão do atalho na sidebar (ShellViewModel.AbrirDevolucao) —
            // devolução mexe em estoque e, opcionalmente, tira dinheiro do caixa.
            modulos.Add(new ModuloTile("Devolução", "Devolver mercadoria de uma venda", SymbolRegular.ArrowUndo24, true,
                new RelayCommand(() =>
                {
                    if (sessao.PodeExcluir("PDV"))
                    {
                        devolucaoWindowFactory().ShowDialog();
                        return;
                    }

                    var janela = autorizacaoWindowFactory("PDV", "Você não tem permissão para registrar uma devolução.");
                    if (janela.ShowDialog() == true)
                        devolucaoWindowFactory().ShowDialog();
                })));
        }

        if (sessao.PodeAcessar("Produtos"))
        {
            modulos.Add(new ModuloTile("Produtos", "Cadastro de produtos", SymbolRegular.Box24, true,
                new RelayCommand(() => navegacao.NavegarPara<ProdutosViewModel>("Produtos"))));
        }

        if (sessao.PodeAcessar("Clientes"))
        {
            modulos.Add(new ModuloTile("Clientes", "Cadastro vinculado a Contas a Receber", SymbolRegular.People24, true,
                new RelayCommand(() => navegacao.NavegarPara<ClientesViewModel>("Clientes"))));
        }

        if (sessao.PodeAcessar("Fornecedores"))
        {
            modulos.Add(new ModuloTile("Fornecedores", "Cadastro vinculado a Contas a Pagar", SymbolRegular.Building24, true,
                new RelayCommand(() => navegacao.NavegarPara<FornecedoresViewModel>("Fornecedores"))));
        }

        if (sessao.PodeAcessar("Contas a Receber"))
        {
            modulos.Add(new ModuloTile("Contas a Receber", "Vendas a prazo e parceladas", SymbolRegular.ReceiptMoney24, true,
                new RelayCommand(() => navegacao.NavegarPara<ContasReceberViewModel>("Contas a receber"))));
        }

        if (sessao.PodeAcessar("Contas a Pagar"))
        {
            modulos.Add(new ModuloTile("Contas a Pagar", "Despesas e obrigações da loja", SymbolRegular.Wallet24, true,
                new RelayCommand(() => navegacao.NavegarPara<ContasPagarViewModel>("Contas a pagar"))));
        }

        if (sessao.PodeAcessar("Caixa"))
        {
            modulos.Add(new ModuloTile("Caixa", "Abertura e fechamento do dia", SymbolRegular.Money24, true,
                new RelayCommand(() => caixaWindowFactory().ShowDialog())));
        }

        if (sessao.PodeAcessar("Vendas"))
        {
            modulos.Add(new ModuloTile("Vendas", "Histórico de vendas do período", SymbolRegular.ReceiptMoney24, true,
                new RelayCommand(() => navegacao.NavegarPara<VendasViewModel>("Vendas"))));
        }

        if (sessao.PodeAcessar("Controle de Validade"))
        {
            modulos.Add(new ModuloTile("Controle de Validade", "Lotes, validade e perdas", SymbolRegular.CalendarClock24, true,
                new RelayCommand(() => navegacao.NavegarPara<LotesViewModel>("Controle de validade"))));
        }

        if (sessao.EhAdministrador)
        {
            modulos.Add(new ModuloTile("Usuários e Permissões", "Funcionários e acessos", SymbolRegular.PeopleSettings24, true,
                new RelayCommand(() => navegacao.NavegarPara<UsuariosViewModel>("Usuários"))));

            modulos.Add(new ModuloTile("Relatórios", "Vendas, financeiro e auditoria", SymbolRegular.DocumentBulletList24, true,
                new RelayCommand(() => navegacao.NavegarPara<RelatoriosViewModel>("Relatórios"))));

            modulos.Add(new ModuloTile("Configurações", "Backup automático e preferências", SymbolRegular.Settings24, true,
                new RelayCommand(() => navegacao.NavegarPara<ConfiguracaoBackupViewModel>("Configurações"))));
        }

        Modulos = new ObservableCollection<ModuloTile>(modulos);
    }
}
