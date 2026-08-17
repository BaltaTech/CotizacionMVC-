using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using CotizacionMVC.ViewModels.Usuarios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CotizacionMVC.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly IUsuarioServicio _usuarioServicio;
        private readonly IUserContextService _userContextService;

        public UsuariosController(
            IUsuarioServicio usuarioServicio,
            IUserContextService userContextService)
        {
            _usuarioServicio = usuarioServicio;
            _userContextService = userContextService;
        }

        public async Task<IActionResult> Indice()
        {
            var usuarios = await _usuarioServicio.ObtenerTodosAsync();
            return View(usuarios);
        }

        public async Task<IActionResult> Crear()
        {
            ViewBag.Roles = await _usuarioServicio.ObtenerRolesAsync();
            ViewBag.Empresas = await _usuarioServicio.ObtenerEmpresasAsync();
            return View(new CrearUsuarioViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearUsuarioViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            var resultado = await _usuarioServicio.CrearAsync(modelo);

            if (!resultado.Exitoso)
            {
                ModelState.AddModelError("", resultado.MensajeError ?? "Error al crear usuario");
                return View(modelo);
            }

            TempData["MensajeExito"] = $"Usuario {resultado.Nombre} creado exitosamente";
            return RedirectToAction(nameof(Indice));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(Guid id)
        {
            var resultado = await _usuarioServicio.CambiarEstadoAsync(id);

            if (!resultado.Exitoso)
            {
                TempData["MensajeError"] = resultado.MensajeError ?? "Error al cambiar estado";
                return RedirectToAction(nameof(Indice));
            }

            TempData["MensajeExito"] = "Estado del usuario actualizado correctamente";
            return RedirectToAction(nameof(Indice));
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(Guid id, string nuevaPassword)
        {
            if (string.IsNullOrWhiteSpace(nuevaPassword) || nuevaPassword.Length < 6)
            {
                TempData["MensajeError"] = "La contraseña debe tener al menos 6 caracteres";
                return RedirectToAction(nameof(Indice));
            }

            var resultado = await _usuarioServicio.CambiarPasswordAsync(id, nuevaPassword);

            if (!resultado.Exitoso)
            {
                TempData["MensajeError"] = resultado.MensajeError ?? "Error al cambiar contraseña";
                return RedirectToAction(nameof(Indice));
            }

            TempData["MensajeExito"] = "Contraseña cambiada exitosamente";
            return RedirectToAction(nameof(Indice));
        }
    }
}