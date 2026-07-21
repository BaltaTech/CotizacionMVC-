using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Servicios.Aplicacion.Dtos.Recepcion;
using CotizacionMVC.ViewModels.Recepcion;

namespace CotizacionMVC.Servicios.Aplicacion.Interfaces
{
    public interface IRecepcionServicio
    {
        Task<ResultadoRegistroCliente> RegistrarClienteAsync(
            RegistrarClienteViewModel modelo, Guid registradoPorId);
        Task<List<Cliente>> BuscarPorTelefonoAsync(string telefono);
        Task<ResultadoRegistroCliente> AsignarVendedorAsync(Guid clienteId, Guid vendedorId);
        Task<ResultadoRegistroCliente> MarcarNoCotizableAsync(Guid clienteId, string motivo, string? comentario);
        Task<IReadOnlyList<ClienteDashboardDto>> ObtenerDashboardAsync(Guid usuarioId);
        Task<List<UltimoRegistroDto>> ObtenerUltimosRegistrosAsync(Guid usuarioId);
        Task<ClienteDetalleRecepcionDto?> ObtenerDetalleClienteAsync(Guid id);
        Task<List<VendedorResumenDto>> ObtenerVendedoresActivosAsync();
        Task<List<EmpresaResumenDto>> ObtenerEmpresasAsync();
    }
}