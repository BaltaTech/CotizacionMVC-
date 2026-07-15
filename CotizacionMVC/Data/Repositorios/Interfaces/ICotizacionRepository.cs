using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Servicios.Aplicacion.Dtos.Cotizacion;

namespace CotizacionMVC.Data.Repositorios.Interfaces
{
    public interface ICotizacionRepository : IRepository<Cotizacion>
    {
        Task<Cotizacion?> ObtenerCompletaPorIdAsync(Guid id);
        Task<IEnumerable<CotizacionResumenDto>> ObtenerTodasConRelacionesAsync();
        Task<IEnumerable<CotizacionResumenDto>> ObtenerPorVendedorAsync(Guid vendedorId);
        Task<string> GenerarSiguienteNumeroAsync();
        Task<Cotizacion?> ObtenerConItemsAsync(Guid id);
        Task<Cotizacion?> ObtenerConClienteAsync(Guid id);
    }
}