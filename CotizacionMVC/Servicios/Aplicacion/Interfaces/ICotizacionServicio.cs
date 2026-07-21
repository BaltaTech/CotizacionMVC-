using CotizacionMVC.Servicios.Aplicacion.Dtos.Cotizacion;

namespace CotizacionMVC.Servicios.Aplicacion.Interfaces
{
    public interface ICotizacionServicio
    {
        Task<IReadOnlyList<CotizacionResumenDto>> ObtenerIndiceAsync(Guid usuarioId);
        Task<IReadOnlyList<LeadResumenDto>> ObtenerLeadsAsync(Guid usuarioId);
        Task<CotizacionDetalleDto?> ObtenerDetalleAsync(Guid id);
        Task<ResultadoCotizacionDto> CrearAsync(CrearCotizacionDto dto);
        Task<ResultadoCotizacionDto> ActualizarAsync(ActualizarCotizacionDto dto);
        Task<ResultadoCotizacionDto> EliminarAsync(Guid id);
        Task<ResultadoCotizacionDto> CambiarEstadoAsync(Guid id, int nuevoEstado);
        Task<byte[]> GenerarPdfAsync(Guid id);
        Task<decimal> CalcularCargaTermicaAsync(decimal area);
        Task<DatosCrearCotizacionDto> ObtenerDatosParaCrearAsync(Guid usuarioId, Guid? leadId);
    }
}