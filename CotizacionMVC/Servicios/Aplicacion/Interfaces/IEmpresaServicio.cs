using CotizacionMVC.Servicios.Aplicacion.Dtos.Empresa;

namespace CotizacionMVC.Servicios.Aplicacion.Interfaces
{
    public interface IEmpresaServicio
    {
        Task<IReadOnlyList<EmpresaResumenDto>> ObtenerTodasAsync();
        Task<EmpresaDetalleDto?> ObtenerPorIdAsync(Guid id);
        Task<EmpresaDetalleDto> ActualizarAsync(ActualizarEmpresaDto dto);
        Task<EmpresaDetalleDto?> ObtenerEmpresaActualAsync();
        Task<bool> TieneAccesoAEmpresaAsync(Guid usuarioId, Guid empresaId);
        Task EstablecerEmpresaActivaAsync(Guid empresaId);
    }
}
