using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Servicios.Aplicacion.Dtos.Empresa;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using CotizacionMVC.ViewModels.Empresa;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CotizacionMVC.Controllers
{
     
    [Authorize]  
    public class EmpresaController : Controller
    {
        private readonly IEmpresaServicio _empresaServicio;
        private readonly UserManager<Usuario> _userManager;
        private readonly IAutorizacionServicio _autorizacionServicio;

        public EmpresaController(
            IEmpresaServicio empresaServicio,
            UserManager<Usuario> userManager,
            IAutorizacionServicio autorizacionServicio)
        {
            _empresaServicio = empresaServicio;
            _userManager = userManager;
            _autorizacionServicio = autorizacionServicio;
        }

        // SOLO ADMIN puede ver el índice de empresas
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Indice()
        {
            var empresas = await _empresaServicio.ObtenerTodasAsync();

            var viewModel = new EmpresaIndiceViewModel
            {
                Empresas = empresas.Select(e => new EmpresaResumenViewModel
                {
                    Id = e.Id,
                    NombreComercial = e.NombreComercial,
                    Slug = e.Slug,
                    Activa = e.Activa,
                    LogoUrl = e.LogoUrl,
                    ColorPrimario = e.ColorPrimario
                }).ToList()
            };

            return View(viewModel);
        }

        // SOLO ADMIN puede editar empresas
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar(Guid? id)
        {
            if (id == null)
                return NotFound("No se proporcionó un identificador de empresa");

            var empresa = await _empresaServicio.ObtenerPorIdAsync(id.Value);

            if (empresa == null)
                return NotFound($"No se encontró la empresa con ID {id}");

            var viewModel = new EmpresaFormViewModel
            {
                Id = empresa.Id,
                NombreComercial = empresa.NombreComercial,
                UtilidadEmpresaPorcentaje = empresa.UtilidadEmpresaPorcentaje,
                UtilidadVendedorPorcentaje = empresa.UtilidadVendedorPorcentaje,
                TelefonoContacto = empresa.TelefonoContacto,
                CorreoContacto = empresa.CorreoContacto,
                LogoUrl = empresa.LogoUrl ?? "",
                ColorPrimario = empresa.ColorPrimario ?? "",
                ColorSecundario = empresa.ColorSecundario ?? "",
                PlantillaPdfNombre = empresa.PlantillaPdfNombre ?? "",
                Eslogan = empresa.Eslogan ?? ""
            };

            return View(viewModel);
        }

        // SOLO ADMIN puede guardar edición
        [HttpPost]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Editar(EmpresaFormViewModel formulario)
        {
            if (!ModelState.IsValid)
                return View(formulario);

            try
            {
                var dto = new ActualizarEmpresaDto
                {
                    Id = formulario.Id,
                    UtilidadEmpresaPorcentaje = formulario.UtilidadEmpresaPorcentaje,
                    UtilidadVendedorPorcentaje = formulario.UtilidadVendedorPorcentaje,
                    TelefonoContacto = formulario.TelefonoContacto,
                    CorreoContacto = formulario.CorreoContacto,
                    LogoUrl = formulario.LogoUrl,
                    ColorPrimario = formulario.ColorPrimario,
                    ColorSecundario = formulario.ColorSecundario,
                    PlantillaPdfNombre = formulario.PlantillaPdfNombre,
                    Eslogan = formulario.Eslogan
                };

                var resultado = await _empresaServicio.ActualizarAsync(dto);

                TempData["MensajeExito"] = $"Empresa {resultado.NombreComercial} actualizada exitosamente";
                return RedirectToAction(nameof(Indice));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                return View(formulario);
            }
        }

        //Admin y Recepción pueden ver todas las empresas
        [HttpPost]
        [Authorize(Roles = "Administrador,Recepcion,Vendedor")]
        public IActionResult VerTodasLasEmpresas(string? returnUrl = null)
        {
            HttpContext.Session.Remove("EmpresaActivaId");
            HttpContext.Session.Remove("EmpresaActivaNombre");
            HttpContext.Session.Remove("EmpresaActivaSlug");
            HttpContext.Session.Remove("EmpresaEsExclusivaTrane");
            HttpContext.Session.Remove("EmpresaColorPrimario");
            HttpContext.Session.Remove("EmpresaColorSecundario");

            TempData["MensajeExito"] = "Viendo todas las empresas";
            return RedirectToLocal(returnUrl);
        }

        // Todos los roles autenticados pueden cambiar de empresa
        [HttpPost]
        public async Task<IActionResult> CambiarEmpresaActiva(Guid empresaId, string? returnUrl = null)
        {
            var usuarioActual = await _userManager.GetUserAsync(User);
            if (usuarioActual == null)
                return RedirectToAction("Login", "Autenticacion");

            var tieneAcceso = await _autorizacionServicio.TieneAccesoAEmpresaAsync(usuarioActual.Id, empresaId);
            if (!tieneAcceso)
            {
                TempData["MensajeError"] = "No tienes acceso a la empresa seleccionada";
                return RedirectToLocal(returnUrl);
            }

            var empresa = await _empresaServicio.ObtenerPorIdAsync(empresaId);

            if (empresa == null)
                return NotFound("No se encontró la empresa seleccionada");

            if (!empresa.Activa)
            {
                TempData["MensajeError"] = "La empresa seleccionada no está activa";
                return RedirectToLocal(returnUrl);
            }

            HttpContext.Session.SetString("EmpresaActivaId", empresa.Id.ToString());
            HttpContext.Session.SetString("EmpresaActivaNombre", empresa.NombreComercial);
            HttpContext.Session.SetString("EmpresaActivaSlug", empresa.Slug);
            HttpContext.Session.SetString("EmpresaEsExclusivaTrane", empresa.EsExclusivaTrane.ToString());
            HttpContext.Session.SetString("EmpresaColorPrimario", empresa.ColorPrimario ?? "#C8102E");
            HttpContext.Session.SetString("EmpresaColorSecundario", empresa.ColorSecundario ?? "#FFFFFF");

            TempData["MensajeExito"] = $"Ahora estás trabajando en: {empresa.NombreComercial}";
            return RedirectToLocal(returnUrl);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult LimpiarEmpresaActiva(string returnUrl)
        {
            HttpContext.Session.Remove("EmpresaActivaId");
            HttpContext.Session.Remove("EmpresaActivaNombre");

            if (!string.IsNullOrEmpty(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Index", "Home");
        }
        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Indice", "Equipo");
        }
    }
}