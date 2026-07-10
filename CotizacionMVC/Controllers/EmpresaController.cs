using CotizacionMVC.Servicios.Aplicacion.Dtos.Empresa;
 using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using CotizacionMVC.ViewModels.Empresa;
using Microsoft.AspNetCore.Mvc;

namespace CotizacionMVC.Controllers
{
    public class EmpresaController : Controller
    {
        private readonly IEmpresaServicio _empresaServicio;

        public EmpresaController(IEmpresaServicio empresaServicio)
        {
            _empresaServicio = empresaServicio;
        }

        // GET: Empresa/Indice
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

        // GET: Empresa/Editar/5
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

        // POST: Empresa/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
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

        // POST: Empresa/CambiarEmpresaActiva/Todas
        [HttpPost]
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

        // POST: Empresa/CambiarEmpresaActiva
        [HttpPost]
        public async Task<IActionResult> CambiarEmpresaActiva(Guid empresaId, string? returnUrl = null)
        {
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

        public async Task<EmpresaDetalleDto?> ObtenerEmpresaActual()
        {
            var empresaIdString = HttpContext.Session.GetString("EmpresaActivaId");

            if (string.IsNullOrEmpty(empresaIdString))
            {
                var empresa = await _empresaServicio.ObtenerEmpresaActualAsync();

                if (empresa != null)
                {
                    HttpContext.Session.SetString("EmpresaActivaId", empresa.Id.ToString());
                    HttpContext.Session.SetString("EmpresaActivaNombre", empresa.NombreComercial);
                    HttpContext.Session.SetString("EmpresaActivaSlug", empresa.Slug);
                    HttpContext.Session.SetString("EmpresaEsExclusivaTrane", empresa.EsExclusivaTrane.ToString());
                    HttpContext.Session.SetString("EmpresaColorPrimario", empresa.ColorPrimario ?? "#C8102E");
                    HttpContext.Session.SetString("EmpresaColorSecundario", empresa.ColorSecundario ?? "#FFFFFF");
                    return empresa;
                }

                return null;
            }

            var empresaId = Guid.Parse(empresaIdString);
            var empresaActiva = await _empresaServicio.ObtenerPorIdAsync(empresaId);

            if (empresaActiva == null || !empresaActiva.Activa)
            {
                HttpContext.Session.Remove("EmpresaActivaId");
                return await ObtenerEmpresaActual();
            }

            return empresaActiva;
        }

        private IActionResult RedirectToLocal(string? returnUrl)
        {
            if (Url.IsLocalUrl(returnUrl))
                return Redirect(returnUrl);

            return RedirectToAction("Indice", "Equipo");
        }
    }
}