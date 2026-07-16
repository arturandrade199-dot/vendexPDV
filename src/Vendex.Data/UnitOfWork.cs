using Microsoft.EntityFrameworkCore;
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
        ContasReceber = new ContaReceberRepository(_contexto);
        ContasReceberPagamentos = new Repository<ContaReceberPagamento>(_contexto);
        ContasPagar = new ContaPagarRepository(_contexto);
        ContasPagarPagamentos = new Repository<ContaPagarPagamento>(_contexto);
        Caixas = new CaixaRepository(_contexto);
        LogsAuditoria = new Repository<LogAuditoria>(_contexto);
        Licencas = new LicencaRepository(_contexto);
        ConfiguracoesBackup = new Repository<ConfiguracaoBackup>(_contexto);
        ConfiguracoesImpressao = new Repository<ConfiguracaoImpressao>(_contexto);
    }

    public IUsuarioRepository Usuarios { get; }
    public IRepository<Modulo> Modulos { get; }
    public IRepository<UsuarioPermissao> UsuarioPermissoes { get; }
    public IProdutoRepository Produtos { get; }
    public IRepository<Cliente> Clientes { get; }
    public IRepository<Fornecedor> Fornecedores { get; }
    public IVendaRepository Vendas { get; }
    public IRepository<VendaPagamento> VendaPagamentos { get; }
    public IContaReceberRepository ContasReceber { get; }
    public IRepository<ContaReceberPagamento> ContasReceberPagamentos { get; }
    public IContaPagarRepository ContasPagar { get; }
    public IRepository<ContaPagarPagamento> ContasPagarPagamentos { get; }
    public ICaixaRepository Caixas { get; }
    public IRepository<LogAuditoria> LogsAuditoria { get; }
    public ILicencaRepository Licencas { get; }
    public IRepository<ConfiguracaoBackup> ConfiguracoesBackup { get; }
    public IRepository<ConfiguracaoImpressao> ConfiguracoesImpressao { get; }

    public Task<int> SalvarAlteracoesAsync() => _contexto.SaveChangesAsync();

    public async Task BackupBancoDadosAsync(string caminhoArquivoDestino)
    {
        var origem = (Microsoft.Data.Sqlite.SqliteConnection)_contexto.Database.GetDbConnection();
        var estavaFechada = origem.State != System.Data.ConnectionState.Open;
        if (estavaFechada) await origem.OpenAsync();

        // Pooling=False: essa conexão de destino é de uso único (só pra receber o backup) — sem
        // isso, o Microsoft.Data.Sqlite mantém o handle nativo aberto no pool mesmo depois do
        // Dispose(), e a pasta temporária não pode ser apagada logo em seguida (arquivo em uso).
        using var destino = new Microsoft.Data.Sqlite.SqliteConnection($"Data Source={caminhoArquivoDestino};Pooling=False");
        destino.Open();
        origem.BackupDatabase(destino);
        destino.Close();

        if (estavaFechada) await origem.CloseAsync();
    }
}
