using CotizacionMVC.Data;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Models.Enums;
using CotizacionMVC.Servicios.Aplicacion.Dtos.Seguimientos;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using CotizacionMVC.ViewModels.Seguimientos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace CotizacionMVC.Controllers
{
    [Authorize]
    public class SeguimientoController : Controller
    {
        private readonly ISeguimientoServicio _seguimientoServicio;
        private readonly ApplicationDbContext _context;

        public SeguimientoController(ISeguimientoServicio seguimientoServicio, ApplicationDbContext context)
        {
            _seguimientoServicio = seguimientoServicio;
            _context = context;
        }

        private Guid GetVendedorId()
        {
            var userIdClaim = User.FindFirstValue(ClaimTypes.NameIdentifier);
            return Guid.Parse(userIdClaim);
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

            var vendedorId = GetVendedorId();

            var empresaId = HttpContext.Session.GetString("EmpresaActivaId");
            Empresa? empresaEntidad = null;

            if (!string.IsNullOrEmpty(empresaId))
                empresaEntidad = await _context.Empresas.FindAsync(Guid.Parse(empresaId));

            if (empresaEntidad == null)
                empresaEntidad = await _context.Empresas.FirstOrDefaultAsync();

            if (empresaEntidad == null)
            {
                TempData["MensajeError"] = "No hay empresa configurada";
                return RedirectToAction("Indice", "Cotizacion");
            }

            var lead = new Lead(
                empresaEntidad,
                modelo.NombreContacto,
                modelo.Telefono,
                CategoriaLead.SinContactar,
                "Prospeccion",
                OrigenLead.Prospeccion,
                modelo.CorreoElectronico);

            var vendedor = await _context.Users.FindAsync(vendedorId);
            if (vendedor != null)
                lead.AsignarVendedor(vendedor);

            if (!string.IsNullOrWhiteSpace(modelo.ProductoBusca))
                lead.EstablecerProducto(modelo.ProductoBusca);

            if (!string.IsNullOrWhiteSpace(modelo.EmpresaCliente))
                lead.ActualizarDatosContacto(null, null, modelo.EmpresaCliente);

            if (!string.IsNullOrWhiteSpace(modelo.Comentarios))
                lead.AgregarComentario(modelo.Comentarios);

            _context.Leads.Add(lead);
            await _context.SaveChangesAsync();

            TempData["MensajeExito"] = "Lead de prospección creado exitosamente";
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
                ResultadoId = 0
            };

            if (leadId.HasValue)
            {
                var lead = await _context.Leads.FirstOrDefaultAsync(l => l.Id == leadId.Value);
                if (lead != null)
                {
                    modelo.TipoSeguimiento = "Lead";
                    modelo.ClienteNombre = lead.NombreContacto;
                    modelo.ClienteTelefono = lead.Telefono;
                    modelo.ClienteCorreo = lead.CorreoElectronico;
                    modelo.EtapaActual = lead.EtapaNegociacion?.ToString() ?? "Sin etapa";
                    modelo.Origen = lead.OrigenLead.ToString();
                    modelo.Referencia = $"Lead: {lead.NombreContacto}";
                }
            }

            if (cotizacionId.HasValue)
            {
                var cotizacion = await _context.Cotizaciones
                    .Include(c => c.Cliente)
                    .FirstOrDefaultAsync(c => c.Id == cotizacionId.Value);
                if (cotizacion != null)
                {
                    modelo.TipoSeguimiento = "Cotización";
                    modelo.ClienteNombre = cotizacion.Cliente?.Nombre;
                    modelo.ClienteTelefono = cotizacion.Cliente?.Contacto?.Telefono;
                    modelo.ClienteCorreo = cotizacion.Cliente?.Contacto?.Correo;
                    modelo.EtapaActual = cotizacion.EtapaNegociacion?.ToString() ?? "Sin etapa";
                    modelo.NumeroCotizacion = cotizacion.NumeroCotizacion;
                    modelo.MontoCotizacion = cotizacion.Total?.Monto;
                    modelo.Referencia = $"Cotización: {cotizacion.NumeroCotizacion}";
                }
            }

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
                    VendedorId = GetVendedorId(),
                    FechaContacto = DateTime.SpecifyKind(modelo.FechaContacto, DateTimeKind.Utc),
                    MedioContacto = modelo.MedioContactoId,
                    Resultado = modelo.ResultadoId,
                    EtapaNegociacion = modelo.EtapaNegociacionId,
                    Notas = modelo.Notas,
                    ProximoContacto = modelo.ProximoContacto.HasValue
                        ? DateTime.SpecifyKind(modelo.ProximoContacto.Value, DateTimeKind.Utc)
                        : null
                };

                await _seguimientoServicio.RegistrarSeguimientoAsync(dto);

                TempData["MensajeExito"] = "Seguimiento registrado exitosamente";

                if (modelo.CotizacionId.HasValue)
                    return RedirectToAction("Detalles", "Cotizacion", new { id = modelo.CotizacionId.Value });

                if (modelo.LeadId.HasValue)
                    return RedirectToAction("Indice", "Cotizacion");

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
            var dashboard = await _seguimientoServicio.ObtenerDashboardAsync(GetVendedorId());
            return Json(dashboard);
        }

        [HttpPost]
        public async Task<IActionResult> MarcarRecordatorio(Guid seguimientoId)
        {
            await _seguimientoServicio.MarcarRecordatorioEnviadoAsync(seguimientoId);
            return Ok();
        }

        [HttpGet]
        public async Task<IActionResult> PorVendedor(Guid vendedorId)
        {
            var seguimientos = await _seguimientoServicio.ObtenerPorVendedorAsync(vendedorId);
            return PartialView("_HistorialSeguimientos", seguimientos);
        }
    }
}