using CotizacionMVC.Servicios.Aplicacion.Dtos.Seguimientos;
using CotizacionMVC.ViewModels.Seguimientos;

namespace CotizacionMVC.Servicios.Aplicacion.Interfaces
{
    public interface ISeguimientoServicio
    {
        Task<SeguimientoListaDto> RegistrarSeguimientoAsync(CrearSeguimientoDto dto);
        Task<IReadOnlyList<SeguimientoListaDto>> ObtenerPorLeadAsync(Guid leadId);
        Task<IReadOnlyList<SeguimientoListaDto>> ObtenerPorCotizacionAsync(Guid cotizacionId);
        Task<IReadOnlyList<SeguimientoListaDto>> ObtenerPorVendedorAsync(Guid vendedorId);
        Task<DashboardVendedorDto> ObtenerDashboardAsync(Guid vendedorId);
        Task MarcarRecordatorioEnviadoAsync(Guid seguimientoId);
         Task<DashboardRecepcionDto> ObtenerDashboardRecepcionAsync();
        Task<ResultadoCrearLead> CrearLeadAsync(CrearLeadViewModel modelo);

    }
}
