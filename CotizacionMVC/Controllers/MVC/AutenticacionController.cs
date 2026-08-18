using CotizacionMVC.Servicios.Aplicacion.Dtos.Autenticacion;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using CotizacionMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CotizacionMVC.Controllers.MVC
{
    [AllowAnonymous]
    public class AutenticacionController : Controller
    {
        private readonly IAutenticacionServicio _autenticacionServicio;

        public AutenticacionController(IAutenticacionServicio autenticacionServicio)
        {
            _autenticacionServicio = autenticacionServicio;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            ViewData["ReturnUrl"] = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel modelo, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            var resultado = await _autenticacionServicio.LoginConCookiesAsync(modelo);

            if (!resultado.Exitoso)
            {
                ModelState.AddModelError(string.Empty, resultado.MensajeError ?? "Error al iniciar sesión");
                return View(modelo);
            }

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Logout()
        {
            await _autenticacionServicio.LogoutAsync();
            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult AccesoDenegado()
        {
            return View();
        }
    }
}