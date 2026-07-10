using CotizacionMVC.Servicios.Aplicacion.Dtos.Cliente;

namespace CotizacionMVC.Servicios.Aplicacion.Interfaces
{
    public interface IClienteServicio
    {
        // Queries
        Task<IReadOnlyList<ClienteResumenDto>> ObtenerTodosAsync(string? termino = null);
        Task<ClienteDetalleDto?> ObtenerPorIdAsync(Guid id);
        Task<ClienteDetalleDto?> ObtenerParaEdicionAsync(Guid id);
        Task<ClienteDetalleDto?> ObtenerParaEliminacionAsync(Guid id);

        // Comandos
        Task<ClienteDetalleDto> CrearAsync(CrearClienteDto dto);
        Task<ClienteDetalleDto> ActualizarAsync(ActualizarClienteDto dto);
        Task<EliminarClienteResultado> EliminarAsync(Guid id);
    }
}
