using Vendex.Domain.Entities;

namespace Vendex.Domain.Interfaces;

public interface ILicencaRepository : IRepository<Licenca>
{
    Task<Licenca?> ObterLicencaAtualAsync();
}
