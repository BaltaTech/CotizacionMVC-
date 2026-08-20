using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Http;
using System.Security.Claims;

namespace CotizacionMVC.Servicios.Aplicacion
{
    public class UserContextService : IUserContextService
    {
        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly UserManager<Usuario> _userManager;

        public UserContextService(
            IHttpContextAccessor httpContextAccessor,
            UserManager<Usuario> userManager)
        {
            _httpContextAccessor = httpContextAccessor;
            _userManager = userManager;
        }

        public async Task<Usuario> GetCurrentUserAsync()
        {
            var user = await _userManager.GetUserAsync(_httpContextAccessor.HttpContext.User);
            if (user == null)
                throw new UnauthorizedAccessException("Usuario no autenticado");
            return user;
        }

        public async Task<Guid> GetCurrentUserIdAsync()
        {
            var user = await GetCurrentUserAsync();
            return user.Id;
        }

        public async Task<string> GetCurrentUserEmailAsync()
        {
            var user = await GetCurrentUserAsync();
            return user.Email ?? string.Empty;
        }

        public async Task<bool> IsUserInRoleAsync(string role)
        {
            var user = await GetCurrentUserAsync();
            return await _userManager.IsInRoleAsync(user, role);
        }

        public async Task<IList<string>> GetCurrentUserRolesAsync()
        {
            var user = await GetCurrentUserAsync();
            return await _userManager.GetRolesAsync(user);
        }
    }
}