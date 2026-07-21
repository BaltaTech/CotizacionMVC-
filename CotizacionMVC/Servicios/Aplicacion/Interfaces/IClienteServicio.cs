using CotizacionMVC.Servicios.Aplicacion.Dtos.Cliente;
 
namespace CotizacionMVC.Servicios.Aplicacion.Interfaces
{
    public interface IClienteServicio
    {
        // Cambiado: recibe usuarioId para filtrar por empresa/rol
        Task<IReadOnlyList<ClienteResumenDto>> ObtenerTodosAsync(Guid usuarioId, string? termino = null);
        Task<ClienteDetalleDto?> ObtenerPorIdAsync(Guid id);
        Task<ClienteDetalleDto?> ObtenerParaEdicionAsync(Guid id);
        Task<ClienteDetalleDto?> ObtenerParaEliminacionAsync(Guid id);
        Task<ClienteDetalleDto> CrearAsync(CrearClienteDto dto);
        Task<ClienteDetalleDto> ActualizarAsync(ActualizarClienteDto dto);
        Task<EliminarClienteResultado> EliminarAsync(Guid id);
    }
}