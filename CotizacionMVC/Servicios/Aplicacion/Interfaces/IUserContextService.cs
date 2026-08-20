using CotizacionMVC.Models.Entidades;

namespace CotizacionMVC.Servicios.Aplicacion.Interfaces
{
    public interface IUserContextService
    {
        Task<Usuario> GetCurrentUserAsync();
        Task<Guid> GetCurrentUserIdAsync();
        Task<string> GetCurrentUserEmailAsync();
        Task<bool> IsUserInRoleAsync(string role);
        Task<IList<string>> GetCurrentUserRolesAsync();
    }
}