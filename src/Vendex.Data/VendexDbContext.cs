using Microsoft.EntityFrameworkCore;
using Vendex.Domain.Entities;

namespace Vendex.Data;

public class VendexDbContext : DbContext
{
    public VendexDbContext(DbContextOptions<VendexDbContext> options) : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Modulo> Modulos => Set<Modulo>();
    public DbSet<UsuarioPermissao> UsuarioPermissoes => Set<UsuarioPermissao>();
    public DbSet<Produto> Produtos => Set<Produto>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<Fornecedor> Fornecedores => Set<Fornecedor>();
    public DbSet<Venda> Vendas => Set<Venda>();
    public DbSet<VendaItem> VendaItens => Set<VendaItem>();
    public DbSet<VendaPagamento> VendaPagamentos => Set<VendaPagamento>();
    public DbSet<ContaReceber> ContasReceber => Set<ContaReceber>();
    public DbSet<ContaReceberPagamento> ContasReceberPagamentos => Set<ContaReceberPagamento>();
    public DbSet<ContaPagar> ContasPagar => Set<ContaPagar>();
    public DbSet<ContaPagarPagamento> ContasPagarPagamentos => Set<ContaPagarPagamento>();
    public DbSet<Caixa> Caixas => Set<Caixa>();
    public DbSet<CaixaAberturaDetalhe> CaixaAberturaDetalhes => Set<CaixaAberturaDetalhe>();
    public DbSet<CaixaFechamentoDetalhe> CaixaFechamentoDetalhes => Set<CaixaFechamentoDetalhe>();
    public DbSet<CaixaMovimentacao> CaixaMovimentacoes => Set<CaixaMovimentacao>();
    public DbSet<LogAuditoria> LogsAuditoria => Set<LogAuditoria>();
    public DbSet<Config> Configs => Set<Config>();
    public DbSet<Licenca> Licencas => Set<Licenca>();
    public DbSet<ConfiguracaoBackup> ConfiguracoesBackup => Set<ConfiguracaoBackup>();
    public DbSet<ConfiguracaoImpressao> ConfiguracoesImpressao => Set<ConfiguracaoImpressao>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Usuario>(e =>
        {
            e.HasIndex(u => u.Login).IsUnique();
        });

        modelBuilder.Entity<Modulo>(e =>
        {
            e.HasIndex(m => m.NomeModulo).IsUnique();
        });

        modelBuilder.Entity<UsuarioPermissao>(e =>
        {
            e.HasOne(p => p.Usuario).WithMany(u => u.Permissoes).HasForeignKey(p => p.UsuarioId);
            e.HasOne(p => p.Modulo).WithMany(m => m.Permissoes).HasForeignKey(p => p.ModuloId);
            e.HasIndex(p => new { p.UsuarioId, p.ModuloId }).IsUnique();
        });

        modelBuilder.Entity<Produto>(e =>
        {
            e.Property(p => p.PrecoCusto).HasPrecision(18, 2);
            e.Property(p => p.PrecoVenda).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Cliente>(e =>
        {
            e.Property(c => c.LimiteCredito).HasPrecision(18, 2);
        });

        modelBuilder.Entity<Venda>(e =>
        {
            e.Property(v => v.ValorTotal).HasPrecision(18, 2);
            e.HasOne(v => v.Usuario).WithMany().HasForeignKey(v => v.UsuarioId);
            e.HasOne(v => v.Cliente).WithMany().HasForeignKey(v => v.ClienteId);
        });

        modelBuilder.Entity<VendaItem>(e =>
        {
            e.Property(i => i.PrecoUnitario).HasPrecision(18, 2);
            e.Property(i => i.PrecoCustoUnitario).HasPrecision(18, 2);
            e.Property(i => i.Subtotal).HasPrecision(18, 2);
            e.HasOne(i => i.Venda).WithMany(v => v.Itens).HasForeignKey(i => i.VendaId);
            e.HasOne(i => i.Produto).WithMany().HasForeignKey(i => i.ProdutoId);
        });

        modelBuilder.Entity<VendaPagamento>(e =>
        {
            e.Property(p => p.Valor).HasPrecision(18, 2);
            e.HasOne(p => p.Venda).WithMany(v => v.Pagamentos).HasForeignKey(p => p.VendaId);
        });

        modelBuilder.Entity<ContaReceber>(e =>
        {
            e.Property(c => c.ValorTotal).HasPrecision(18, 2);
            e.HasOne(c => c.Cliente).WithMany().HasForeignKey(c => c.ClienteId);
            e.HasOne(c => c.Venda).WithMany().HasForeignKey(c => c.VendaId);
        });

        modelBuilder.Entity<ContaReceberPagamento>(e =>
        {
            e.Property(p => p.ValorPago).HasPrecision(18, 2);
            e.HasOne(p => p.ContaReceber).WithMany(c => c.Pagamentos).HasForeignKey(p => p.ContaReceberId);
        });

        modelBuilder.Entity<ContaPagar>(e =>
        {
            e.Property(c => c.ValorTotal).HasPrecision(18, 2);
            e.HasOne(c => c.Fornecedor).WithMany().HasForeignKey(c => c.FornecedorId);
        });

        modelBuilder.Entity<ContaPagarPagamento>(e =>
        {
            e.Property(p => p.ValorPago).HasPrecision(18, 2);
            e.HasOne(p => p.ContaPagar).WithMany(c => c.Pagamentos).HasForeignKey(p => p.ContaPagarId);
        });

        modelBuilder.Entity<Caixa>(e =>
        {
            e.Property(c => c.ValorAberturaTotal).HasPrecision(18, 2);
            e.Property(c => c.ValorFechamentoTotal).HasPrecision(18, 2);
            e.Property(c => c.FaturamentoTotal).HasPrecision(18, 2);
            e.Property(c => c.CustoTotal).HasPrecision(18, 2);
            e.Property(c => c.LucroTotal).HasPrecision(18, 2);
            e.HasOne(c => c.UsuarioAbertura).WithMany().HasForeignKey(c => c.UsuarioAberturaId).OnDelete(DeleteBehavior.Restrict);
            e.HasOne(c => c.UsuarioFechamento).WithMany().HasForeignKey(c => c.UsuarioFechamentoId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<CaixaAberturaDetalhe>(e =>
        {
            e.Property(d => d.TipoCedula).HasPrecision(18, 2);
            e.Property(d => d.Subtotal).HasPrecision(18, 2);
            e.HasOne(d => d.Caixa).WithMany(c => c.AberturaDetalhes).HasForeignKey(d => d.CaixaId);
        });

        modelBuilder.Entity<CaixaFechamentoDetalhe>(e =>
        {
            e.Property(d => d.TipoCedula).HasPrecision(18, 2);
            e.Property(d => d.Subtotal).HasPrecision(18, 2);
            e.HasOne(d => d.Caixa).WithMany(c => c.FechamentoDetalhes).HasForeignKey(d => d.CaixaId);
        });

        modelBuilder.Entity<CaixaMovimentacao>(e =>
        {
            e.Property(m => m.Valor).HasPrecision(18, 2);
            e.HasOne(m => m.Caixa).WithMany(c => c.Movimentacoes).HasForeignKey(m => m.CaixaId);
            e.HasOne(m => m.Usuario).WithMany().HasForeignKey(m => m.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<LogAuditoria>(e =>
        {
            e.HasOne(l => l.Usuario).WithMany().HasForeignKey(l => l.UsuarioId).OnDelete(DeleteBehavior.Restrict);
        });
    }
}
