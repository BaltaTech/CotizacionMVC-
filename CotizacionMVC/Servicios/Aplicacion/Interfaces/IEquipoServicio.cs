using CotizacionMVC.Servicios.Aplicacion.Dtos.Equipo;

namespace CotizacionMVC.Servicios.Aplicacion.Interfaces
{
    public interface IEquipoServicio
    {
        Task<IReadOnlyList<EquipoResumenDto>> ObtenerTodosAsync();
        Task<EquipoDetalleDto?> ObtenerPorIdAsync(Guid id);
        Task<EquipoDetalleDto> CrearAsync(CrearEquipoDto dto);
        Task<EquipoDetalleDto> ActualizarAsync(ActualizarEquipoDto dto);
        Task<EliminarEquipoResultado> EliminarAsync(Guid id);
    }
}
