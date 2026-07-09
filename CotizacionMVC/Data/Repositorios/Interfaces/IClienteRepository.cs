using CotizacionMVC.Models.Entidades;

namespace CotizacionMVC.Data.Repositorios.Interfaces
{
    public interface IClienteRepository : IRepository<Cliente>
    {
        Task<IEnumerable<Cliente>> ObtenerTodosOrdenadosAsync();
        Task<string> GenerarFolioAsync();
        Task<Cliente?> ExisteTelefonoAsync(string telefono);
        Task<IEnumerable<Cliente>> ObtenerParaCotizacionAsync();

    }
}
