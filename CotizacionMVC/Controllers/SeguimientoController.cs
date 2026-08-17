using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Models.Enums;
using CotizacionMVC.Servicios.Aplicacion.Dtos.Seguimientos;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using CotizacionMVC.ViewModels.Seguimientos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CotizacionMVC.Controllers
{
    [Authorize]
    public class SeguimientoController : Controller
    {
        private readonly ISeguimientoServicio _seguimientoServicio;
        private readonly IUserContextService _userContextService;

        public SeguimientoController(
            ISeguimientoServicio seguimientoServicio,
            IUserContextService userContextService)
        {
            _seguimientoServicio = seguimientoServicio;
            _userContextService = userContextService;
        }

        [HttpGet]
        public IActionResult CrearLead()
        {
            return View(new CrearLeadViewModel());
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> CrearLead(CrearLeadViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            var resultado = await _seguimientoServicio.CrearLeadAsync(modelo);

            if (!resultado.Exitoso)
            {
                TempData["MensajeError"] = resultado.MensajeError;
                return View(modelo);
            }

            TempData["MensajeExito"] = "Lead creado exitosamente";
            return RedirectToAction("Indice", "Cotizacion");
        }

        [HttpGet]
        public async Task<IActionResult> Crear(Guid? leadId, Guid? cotizacionId)
        {
            if (!leadId.HasValue && !cotizacionId.HasValue)
                return RedirectToAction("Indice", "Cotizacion");

            var modelo = new CrearSeguimientoViewModel
            {
                LeadId = leadId,
                CotizacionId = cotizacionId,
                FechaContacto = DateTime.Now,
                MedioContactoId = 0,
                ResultadoId = 0,
                Referencia = null,
                TipoSeguimiento = leadId.HasValue ? "Lead" : "Cotización"
            };

            return View(modelo);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearSeguimientoViewModel modelo)
        {
            if (!ModelState.IsValid)
                return View(modelo);

            try
            {
                var dto = new CrearSeguimientoDto
                {
                    LeadId = modelo.LeadId,
                    CotizacionId = modelo.CotizacionId,
                    VendedorId = await _userContextService.GetCurrentUserIdAsync(),
                    FechaContacto = DateTime.SpecifyKind(modelo.FechaContacto, DateTimeKind.Utc),
                    MedioContacto = modelo.MedioContactoId,
                    Resultado = modelo.ResultadoId,
                    Notas = modelo.Notas,
                    ProximoContacto = modelo.ProximoContacto.HasValue
                        ? DateTime.SpecifyKind(modelo.ProximoContacto.Value, DateTimeKind.Utc)
                        : null
                };

                await _seguimientoServicio.RegistrarSeguimientoAsync(dto);

                TempData["MensajeExito"] = "Seguimiento registrado exitosamente";

                if (modelo.CotizacionId.HasValue)
                    return RedirectToAction("Detalles", "Cotizacion", new { id = modelo.CotizacionId.Value });

                return RedirectToAction("Indice", "Cotizacion");
            }
            catch (UnauthorizedAccessException)
            {
                TempData["MensajeError"] = "No tienes permiso para registrar seguimientos aquí";
                return RedirectToAction("Indice", "Cotizacion");
            }
            catch (InvalidOperationException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(modelo);
            }
        }

        [HttpGet]
        public async Task<IActionResult> PorLead(Guid leadId)
        {
            var seguimientos = await _seguimientoServicio.ObtenerPorLeadAsync(leadId);
            return PartialView("_HistorialSeguimientos", seguimientos);
        }

        [HttpGet]
        public async Task<IActionResult> PorCotizacion(Guid cotizacionId)
        {
            var seguimientos = await _seguimientoServicio.ObtenerPorCotizacionAsync(cotizacionId);
            return PartialView("_HistorialSeguimientos", seguimientos);
        }

        [HttpGet]
        public IActionResult Dashboard()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> MiDashboard()
        {
            var vendedorId = await _userContextService.GetCurrentUserIdAsync();
            var dashboard = await _seguimientoServicio.ObtenerDashboardAsync(vendedorId);
            return Json(dashboard);
        }

        [HttpPost]
        public async Task<IActionResult> MarcarRecordatorio(Guid seguimientoId)
        {
            await _seguimientoServicio.MarcarRecordatorioEnviadoAsync(seguimientoId);
            return Ok();
        }
    }
}