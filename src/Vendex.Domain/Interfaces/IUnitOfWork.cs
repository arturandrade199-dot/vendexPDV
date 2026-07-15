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
    IContaReceberRepository ContasReceber { get; }
    IRepository<ContaReceberPagamento> ContasReceberPagamentos { get; }
    IContaPagarRepository ContasPagar { get; }
    IRepository<ContaPagarPagamento> ContasPagarPagamentos { get; }
    ICaixaRepository Caixas { get; }
    IRepository<LogAuditoria> LogsAuditoria { get; }
    ILicencaRepository Licencas { get; }
    IRepository<ConfiguracaoBackup> ConfiguracoesBackup { get; }

    Task<int> SalvarAlteracoesAsync();
    Task BackupBancoDadosAsync(string caminhoArquivoDestino);
}
