using CotizacionMVC.Models.Entidades;

namespace CotizacionMVC.Data.Repositorios.Interfaces
{
    public interface IInstalacionRepository : IRepository<Instalacion>
    {
        Task<IEnumerable<Instalacion>> ObtenerActivasAsync();
    }
}
