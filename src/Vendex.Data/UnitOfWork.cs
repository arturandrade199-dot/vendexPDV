using Vendex.Data.Repositories;
using Vendex.Domain.Entities;
using Vendex.Domain.Interfaces;

namespace Vendex.Data;

public class UnitOfWork : IUnitOfWork
{
    private readonly VendexDbContext _contexto;

    public UnitOfWork(VendexDbContext contexto)
    {
        _contexto = contexto;
        Usuarios = new UsuarioRepository(_contexto);
        Modulos = new Repository<Modulo>(_contexto);
        UsuarioPermissoes = new Repository<UsuarioPermissao>(_contexto);
        Produtos = new ProdutoRepository(_contexto);
        Clientes = new Repository<Cliente>(_contexto);
        Fornecedores = new Repository<Fornecedor>(_contexto);
        Vendas = new VendaRepository(_contexto);
        VendaPagamentos = new Repository<VendaPagamento>(_contexto);
        ContasReceber = new Repository<ContaReceber>(_contexto);
        ContasReceberPagamentos = new Repository<ContaReceberPagamento>(_contexto);
        ContasPagar = new Repository<ContaPagar>(_contexto);
        ContasPagarPagamentos = new Repository<ContaPagarPagamento>(_contexto);
        Caixas = new CaixaRepository(_contexto);
        LogsAuditoria = new Repository<LogAuditoria>(_contexto);
        Licencas = new LicencaRepository(_contexto);
    }

    public IUsuarioRepository Usuarios { get; }
    public IRepository<Modulo> Modulos { get; }
    public IRepository<UsuarioPermissao> UsuarioPermissoes { get; }
    public IProdutoRepository Produtos { get; }
    public IRepository<Cliente> Clientes { get; }
    public IRepository<Fornecedor> Fornecedores { get; }
    public IVendaRepository Vendas { get; }
    public IRepository<VendaPagamento> VendaPagamentos { get; }
    public IRepository<ContaReceber> ContasReceber { get; }
    public IRepository<ContaReceberPagamento> ContasReceberPagamentos { get; }
    public IRepository<ContaPagar> ContasPagar { get; }
    public IRepository<ContaPagarPagamento> ContasPagarPagamentos { get; }
    public ICaixaRepository Caixas { get; }
    public IRepository<LogAuditoria> LogsAuditoria { get; }
    public ILicencaRepository Licencas { get; }

    public Task<int> SalvarAlteracoesAsync() => _contexto.SaveChangesAsync();
}
