using CotizacionMVC.Servicios.Aplicacion.Dtos.Cotizacion;

namespace CotizacionMVC.Servicios.Aplicacion.Interfaces
{
    public interface ICotizacionServicio
    {
        Task<IReadOnlyList<CotizacionResumenDto>> ObtenerIndiceAsync(Guid? vendedorId, Guid? empresaId, bool esAdmin);
        Task<CotizacionDetalleDto?> ObtenerDetalleAsync(Guid id);
        Task<ResultadoCotizacionDto> CrearAsync(CrearCotizacionDto dto);
        Task<ResultadoCotizacionDto> ActualizarAsync(ActualizarCotizacionDto dto);
        Task<ResultadoCotizacionDto> EliminarAsync(Guid id);
        Task<ResultadoCotizacionDto> CambiarEstadoAsync(Guid id, int nuevoEstado);
        Task<byte[]> GenerarPdfAsync(Guid id);
        Task<decimal> CalcularCargaTermicaAsync(decimal area);
        Task<IReadOnlyList<LeadResumenDto>> ObtenerLeadsDelVendedorAsync(Guid vendedorId);
        Task<DatosCrearCotizacionDto> ObtenerDatosParaCrearAsync(Guid usuarioId, bool esVendedor, Guid? leadId);
    }
}