using System.Collections.ObjectModel;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Vendex.App.Navigation;
using Wpf.Ui.Controls;

namespace Vendex.App.ViewModels;

public class MenuViewModel : ObservableObject
{
    public ObservableCollection<ModuloTile> Modulos { get; }

    public MenuViewModel(INavigationService navegacao, Func<PdvWindow> pdvWindowFactory)
    {
        Modulos = new ObservableCollection<ModuloTile>
        {
            new("PDV (Venda)", "Registrar vendas no balcão", SymbolRegular.Cart24, true,
                new RelayCommand(() => pdvWindowFactory().ShowDialog())),
            new("Produtos", "Cadastro de produtos", SymbolRegular.Box24, true,
                new RelayCommand(() => navegacao.NavegarPara<ProdutosViewModel>("Produtos"))),
            new("Clientes e Fornecedores", "Cadastros vinculados a contas", SymbolRegular.People24, false, null),
            new("Contas a Receber", "Vendas a prazo e parceladas", SymbolRegular.ReceiptMoney24, false, null),
            new("Contas a Pagar", "Despesas e obrigações da loja", SymbolRegular.Wallet24, true,
                new RelayCommand(() => navegacao.NavegarPara<ContasPagarViewModel>("Contas a pagar"))),
            new("Caixa", "Abertura e fechamento do dia", SymbolRegular.Money24, false, null),
            new("Usuários e Permissões", "Funcionários e acessos", SymbolRegular.PeopleSettings24, false, null),
            new("Relatórios", "Vendas, financeiro e auditoria", SymbolRegular.DocumentBulletList24, false, null),
            new("Configurações", "Dados da loja e preferências", SymbolRegular.Settings24, false, null),
        };
    }
}
