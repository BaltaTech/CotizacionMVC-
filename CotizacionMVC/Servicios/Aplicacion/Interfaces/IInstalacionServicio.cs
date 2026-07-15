using CotizacionMVC.ViewModels.Instalacion;

namespace CotizacionMVC.Servicios.Aplicacion.Interfaces
{
    public interface IInstalacionServicio
    {
        Task<InstalacionCatalogoViewModel> ObtenerCatalogoAsync();
    }
}