using CotizacionMVC.Models.Entidades;

namespace CotizacionMVC.Data.Repositorios.Interfaces
{
    public interface ICotizacionRepository : IRepository<Cotizacion>
    {
        Task<Cotizacion?> ObtenerCompletaPorIdAsync(Guid id);

        Task<IEnumerable<Cotizacion>> ObtenerTodasConRelacionesAsync();

        Task<IEnumerable<Cotizacion>> ObtenerPorVendedorAsync(Guid vendedorId);

        Task<string> GenerarSiguienteNumeroAsync();

        Task<Cotizacion?> ObtenerConItemsAsync(Guid id);

        Task<Cotizacion?> ObtenerConClienteAsync(Guid id);
    }
}
