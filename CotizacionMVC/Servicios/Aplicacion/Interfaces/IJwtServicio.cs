using System.Security.Claims;
using CotizacionMVC.Servicios.Aplicacion.Dtos.Auth;

namespace CotizacionMVC.Servicios.Aplicacion.Interfaces
{
    public interface IJwtServicio
    {
        string GenerarToken(Guid usuarioId, string email, string nombreCompleto, IList<string> roles, Guid? empresaId = null);

        TokenResultDto GenerarTokenCompleto(Guid usuarioId, string email, string nombreCompleto, IList<string> roles, Guid? empresaId = null);

        ClaimsPrincipal? ValidarToken(string token);
        Guid? ObtenerUsuarioIdDesdeToken(string token);
        string GenerarRefreshToken();
        bool TokenHaExpirado(string token);
    }
}