using CotizacionMVC.Servicios.Aplicacion.Dtos.Autenticacion;
using CotizacionMVC.ViewModels;

namespace CotizacionMVC.Servicios.Aplicacion.Interfaces
{
    public interface IAutenticacionServicio
    {
        Task<LoginResultDto> LoginAsync(LoginRequestDto request);
        Task LogoutAsync();
        Task<LoginResultDto> LoginConCookiesAsync(LoginViewModel modelo);
    }
}