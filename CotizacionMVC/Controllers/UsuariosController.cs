using CotizacionMVC.Models.Entidades;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class UsuariosController : Controller
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;

        public UsuariosController(
            UserManager<Usuario> userManager,
            RoleManager<IdentityRole<Guid>> roleManager)
        {
            _userManager = userManager;
            _roleManager = roleManager;
        }

        // GET: Usuarios
        public async Task<IActionResult> Indice()
        {
            var usuarios = await _userManager.Users
                .OrderBy(u => u.NombreCompleto)
                .ToListAsync();

            var usuariosConRoles = new List<UsuarioConRolesViewModel>();

            foreach (var usuario in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(usuario);
                usuariosConRoles.Add(new UsuarioConRolesViewModel
                {
                    Id = usuario.Id,
                    NombreCompleto = usuario.NombreCompleto,
                    Email = usuario.Email!,
                    Activo = usuario.Activo,
                    FechaRegistro = usuario.FechaRegistro,
                    UltimoAcceso = usuario.UltimoAcceso,
                    Roles = roles.ToList()
                });
            }

            return View(usuariosConRoles);
        }

        // GET: Usuarios/Crear
        public async Task<IActionResult> Crear()
        {
            ViewBag.Roles = await _roleManager.Roles
                .Select(r => r.Name)
                .ToListAsync();
            return View();
        }

        // POST: Usuarios/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearUsuarioViewModel modelo)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
                return View(modelo);
            }

            var usuario = new Usuario(modelo.NombreCompleto, modelo.Email);
            var resultado = await _userManager.CreateAsync(usuario, modelo.Password);

            if (resultado.Succeeded)
            {
                if (!string.IsNullOrEmpty(modelo.Rol))
                    await _userManager.AddToRoleAsync(usuario, modelo.Rol);

                TempData["MensajeExito"] = $"Usuario {usuario.NombreCompleto} creado exitosamente";
                return RedirectToAction(nameof(Indice));
            }

            foreach (var error in resultado.Errors)
                ModelState.AddModelError("", error.Description);

            ViewBag.Roles = await _roleManager.Roles.Select(r => r.Name).ToListAsync();
            return View(modelo);
        }

        // POST: Usuarios/CambiarEstado
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarEstado(Guid id)
        {
            var usuario = await _userManager.FindByIdAsync(id.ToString());
            if (usuario == null) return NotFound();

            if (usuario.Activo)
                usuario.Desactivar();
            else
                usuario.Activar();

            await _userManager.UpdateAsync(usuario);
            TempData["MensajeExito"] = $"Usuario {(usuario.Activo ? "activado" : "desactivado")} correctamente";
            return RedirectToAction(nameof(Indice));
        }

        // POST: Usuarios/CambiarPassword
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CambiarPassword(Guid id, string nuevaPassword)
        {
            if (string.IsNullOrWhiteSpace(nuevaPassword) || nuevaPassword.Length < 6)
            {
                TempData["MensajeError"] = "La contraseña debe tener al menos 6 caracteres";
                return RedirectToAction(nameof(Indice));
            }

            var usuario = await _userManager.FindByIdAsync(id.ToString());
            if (usuario == null) return NotFound();

            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
            var resultado = await _userManager.ResetPasswordAsync(usuario, token, nuevaPassword);

            if (resultado.Succeeded)
                TempData["MensajeExito"] = "Contraseña cambiada exitosamente";
            else
                TempData["MensajeError"] = "Error al cambiar la contraseña";

            return RedirectToAction(nameof(Indice));
        }
    }

    // ViewModels
    public class UsuarioConRolesViewModel
    {
        public Guid Id { get; set; }
        public string NombreCompleto { get; set; } = "";
        public string Email { get; set; } = "";
        public bool Activo { get; set; }
        public DateTime FechaRegistro { get; set; }
        public DateTime? UltimoAcceso { get; set; }
        public List<string> Roles { get; set; } = new();
        public string RolesDisplay => string.Join(", ", Roles);
    }

    public class CrearUsuarioViewModel
    {
        [System.ComponentModel.DataAnnotations.Required]
        public string NombreCompleto { get; set; } = "";

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.EmailAddress]
        public string Email { get; set; } = "";

        [System.ComponentModel.DataAnnotations.Required]
        [System.ComponentModel.DataAnnotations.MinLength(6)]
        public string Password { get; set; } = "";

        public string? Rol { get; set; }
    }
}