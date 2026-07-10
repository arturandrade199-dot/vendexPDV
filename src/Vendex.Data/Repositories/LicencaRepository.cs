using Microsoft.EntityFrameworkCore;
using Vendex.Domain.Entities;
using Vendex.Domain.Interfaces;

namespace Vendex.Data.Repositories;

public class LicencaRepository : Repository<Licenca>, ILicencaRepository
{
    public LicencaRepository(VendexDbContext contexto) : base(contexto)
    {
    }

    public Task<Licenca?> ObterLicencaAtualAsync() => DbSet.FirstOrDefaultAsync();
}
