using Vendex.Domain.Entities;

namespace Vendex.Domain.Interfaces;

public interface IUnitOfWork
{
    IUsuarioRepository Usuarios { get; }
    IRepository<Modulo> Modulos { get; }
    IRepository<UsuarioPermissao> UsuarioPermissoes { get; }
    IProdutoRepository Produtos { get; }
    IRepository<Cliente> Clientes { get; }
    IRepository<Fornecedor> Fornecedores { get; }
    IVendaRepository Vendas { get; }
    IRepository<VendaPagamento> VendaPagamentos { get; }
    IRepository<ContaReceber> ContasReceber { get; }
    IRepository<ContaReceberPagamento> ContasReceberPagamentos { get; }
    IRepository<ContaPagar> ContasPagar { get; }
    IRepository<ContaPagarPagamento> ContasPagarPagamentos { get; }
    ICaixaRepository Caixas { get; }
    IRepository<LogAuditoria> LogsAuditoria { get; }
    ILicencaRepository Licencas { get; }

    Task<int> SalvarAlteracoesAsync();
}
