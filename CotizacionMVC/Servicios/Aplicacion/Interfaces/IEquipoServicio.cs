using CotizacionMVC.Models.Enums;
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
        Task<IReadOnlyList<string>> ObtenerSistemasAsync();
        Task<IReadOnlyList<string>> ObtenerModosPorSistemaAsync(string sistema);
        Task<IReadOnlyList<EquipoResumenDto>> ObtenerPorSistemaYModoAsync(string sistema, string modo);
        Task<IReadOnlyList<string>> ObtenerSistemasPorMarcaAsync(TipoMarca marca);
    }
}
