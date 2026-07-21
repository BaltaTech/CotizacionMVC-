using CotizacionMVC.Models.Entidades;

namespace CotizacionMVC.Data.Repositorios.Interfaces
{
    public interface IEmpresaRepository : IRepository<Empresa>
    {
        Task<Empresa?> ObtenerActivaAsync();
        IQueryable<Empresa> ObtenerQueryable();
    }
}
