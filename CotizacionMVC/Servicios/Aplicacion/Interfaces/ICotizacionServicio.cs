using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Servicios.Aplicacion.Dtos.Cotizacion;
using CotizacionMVC.ViewModels.Instalacion;

public interface ICotizacionServicio
{
    Task<IReadOnlyList<CotizacionResumenDto>> ObtenerIndiceAsync();                 
    Task<IReadOnlyList<LeadResumenDto>> ObtenerLeadsAsync();                       
    Task<DatosCrearCotizacionDto> ObtenerDatosParaCrearAsync(Guid? leadId);        
    Task<CotizacionDetalleDto?> ObtenerDetalleAsync(Guid id);
    Task<ResultadoCotizacionDto> CrearAsync(CrearCotizacionDto dto);
    Task<ResultadoCotizacionDto> ActualizarAsync(ActualizarCotizacionDto dto);
    Task<ResultadoCotizacionDto> EliminarAsync(Guid id);
    Task<ResultadoCotizacionDto> CambiarEstadoAsync(Guid id, int nuevoEstado);
    Task<byte[]> GenerarPdfAsync(Guid id);
    Task<decimal> CalcularCargaTermicaAsync(decimal area);
    Task<Empresa?> ObtenerEmpresaActivaAsync();
    Task<InstalacionCatalogoViewModel> ObtenerCatalogoInstalacionesAsync();
}