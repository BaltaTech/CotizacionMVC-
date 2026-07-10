using CotizacionMVC.Models.Entidades;
namespace CotizacionMVC.Data.Repositorios.Interfaces
{
    public interface IEquipoRepository : IRepository<Equipo>
    {
        Task<IEnumerable<Equipo>> ObtenerTodosOrdenadosAsync();
        IQueryable<Equipo> ObtenerQueryable();
        Task<bool> EstaEnUsoAsync(Guid id);
    }
}
