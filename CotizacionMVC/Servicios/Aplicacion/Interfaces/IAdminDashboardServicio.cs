using CotizacionMVC.Servicios.Aplicacion.Dtos.AdminDashboard;

namespace CotizacionMVC.Servicios.Aplicacion.Interfaces
{
    public interface IAdminDashboardServicio
    {
        Task<AdminDashboardDto> ObtenerDashboardAsync(Guid? empresaId = null);
    }
}
