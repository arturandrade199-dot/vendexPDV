using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;

namespace Vendex.Data;

/// <summary>
/// Usada apenas em tempo de design pelo `dotnet ef` (migrations). A aplicação real
/// configura o DbContext via DI em Vendex.App, com o caminho do banco vindo do appsettings.json.
/// </summary>
public class VendexDbContextFactory : IDesignTimeDbContextFactory<VendexDbContext>
{
    public VendexDbContext CreateDbContext(string[] args)
    {
        var optionsBuilder = new DbContextOptionsBuilder<VendexDbContext>();
        optionsBuilder.UseSqlite("Data Source=vendex.db");
        return new VendexDbContext(optionsBuilder.Options);
    }
}
