using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using CotizacionMVC.Servicios.Configuracion;
using CotizacionMVC.ViewModels.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace CotizacionMVC.Controllers.MVC
{
    [ApiController]
    [Route("api/[controller]")]
    [AllowAnonymous]
    public class AuthController : ControllerBase
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;
        private readonly IJwtServicio _jwtService;
        private readonly IUserContextService _userContextService;
        private readonly ILogger<AuthController> _logger;
        private readonly JwtConfig _jwtConfig;

        public AuthController(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager,
            IJwtServicio jwtService,
            IUserContextService userContextService,
            ILogger<AuthController> logger,
            IOptions<JwtConfig> jwtConfig)
        {
            _userManager = userManager;
            _signInManager = signInManager;
            _jwtService = jwtService;
            _userContextService = userContextService;
            _logger = logger;
            _jwtConfig = jwtConfig.Value;
        }

        [HttpPost("login")]
        public async Task<ActionResult<AuthResponse>> Login([FromBody] LoginRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(AuthResponse.CreateError("Datos de inicio de sesión inválidos"));
                }

                var usuario = await _userManager.FindByEmailAsync(request.Email);
                if (usuario == null)
                {
                    _logger.LogWarning($"Intento de login con email no registrado: {request.Email}");
                    return Unauthorized(AuthResponse.CreateError("Correo electrónico o contraseña incorrectos"));
                }

                if (!usuario.Activo)
                {
                    _logger.LogWarning($"Intento de login de usuario inactivo: {request.Email}");
                    return Unauthorized(AuthResponse.CreateError("Usuario inactivo. Contacte al administrador"));
                }

                var resultado = await _signInManager.PasswordSignInAsync(
                    usuario.UserName!,
                    request.Password,
                    request.Recordarme,
                    lockoutOnFailure: false);

                if (!resultado.Succeeded)
                {
                    _logger.LogWarning($"Intento de login con contraseña incorrecta: {request.Email}");
                    return Unauthorized(AuthResponse.CreateError("Correo electrónico o contraseña incorrectos"));
                }

                usuario.RegistrarAcceso();
                await _userManager.UpdateAsync(usuario);

                var roles = await _userManager.GetRolesAsync(usuario);

                var tokenResult = _jwtService.GenerarTokenCompleto(
                    usuario.Id,
                    usuario.Email!,
                    usuario.NombreCompleto,
                    roles,
                    null);

                _logger.LogInformation($"Login exitoso: {request.Email}");

                return Ok(AuthResponse.CreateSuccess(
                    tokenResult.Token,
                    tokenResult.RefreshToken,
                    tokenResult.ExpiresInSeconds));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, $"Error en login: {request.Email}");
                return StatusCode(500, AuthResponse.CreateError("Error interno del servidor"));
            }
        }

        [HttpPost("refresh")]
        public async Task<ActionResult<AuthResponse>> Refresh([FromBody] RefreshRequest request)
        {
            try
            {
                if (!ModelState.IsValid)
                {
                    return BadRequest(AuthResponse.CreateError("El refresh token es obligatorio"));
                }

                return Unauthorized(AuthResponse.CreateError("Refresh token no válido o expirado"));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al refrescar token");
                return StatusCode(500, AuthResponse.CreateError("Error interno del servidor"));
            }
        }

        [HttpPost("logout")]
        [Authorize]
        public async Task<IActionResult> Logout()
        {
            try
            {
                var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
                if (!string.IsNullOrEmpty(userId))
                {
                    _logger.LogInformation($"Cierre de sesión del usuario: {userId}");
                }

                await _signInManager.SignOutAsync();

                return Ok(new { message = "Sesión cerrada exitosamente" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al cerrar sesión");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }

        [HttpGet("me")]
        [Authorize]
        public async Task<ActionResult<object>> GetCurrentUser()
        {
            try
            {
                var user = await _userContextService.GetCurrentUserAsync();
                var roles = await _userManager.GetRolesAsync(user);

                return Ok(new
                {
                    user.Id,
                    user.Email,
                    user.NombreCompleto,
                    Roles = roles,
                    user.Activo
                });
            }
            catch (UnauthorizedAccessException)
            {
                return Unauthorized(new { message = "Usuario no autenticado" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error al obtener usuario actual");
                return StatusCode(500, new { message = "Error interno del servidor" });
            }
        }
    }
}