using CotizacionMVC.Servicios.Aplicacion.Dtos.Seguimientos;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using CotizacionMVC.ViewModels.Seguimientos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CotizacionMVC.Controllers.API
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class SeguimientoController : ControllerBase
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

        // ========== GET: Dashboard del vendedor ==========
        [HttpGet("dashboard")]
        public async Task<ActionResult<DashboardVendedorDto>> GetDashboard()
        {
            var vendedorId = await _userContextService.GetCurrentUserIdAsync();
            var dashboard = await _seguimientoServicio.ObtenerDashboardAsync(vendedorId);
            return Ok(dashboard);
        }

        // ========== GET: Seguimientos por Lead ==========
        [HttpGet("lead/{leadId}")]
        public async Task<ActionResult<IReadOnlyList<SeguimientoListaDto>>> GetByLead(Guid leadId)
        {
            var seguimientos = await _seguimientoServicio.ObtenerPorLeadAsync(leadId);
            return Ok(seguimientos);
        }

        // ========== GET: Seguimientos por Cotización ==========
        [HttpGet("cotizacion/{cotizacionId}")]
        public async Task<ActionResult<IReadOnlyList<SeguimientoListaDto>>> GetByCotizacion(Guid cotizacionId)
        {
            var seguimientos = await _seguimientoServicio.ObtenerPorCotizacionAsync(cotizacionId);
            return Ok(seguimientos);
        }

        // ========== POST: Crear seguimiento ==========
        [HttpPost]
        public async Task<ActionResult<SeguimientoListaDto>> Post([FromBody] CrearSeguimientoViewModel modelo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var vendedorId = await _userContextService.GetCurrentUserIdAsync();

                var dto = new CrearSeguimientoDto
                {
                    LeadId = modelo.LeadId,
                    CotizacionId = modelo.CotizacionId,
                    VendedorId = vendedorId,
                    FechaContacto = modelo.FechaContacto,
                    MedioContacto = modelo.MedioContactoId,
                    Resultado = modelo.ResultadoId,
                    Notas = modelo.Notas,
                    ProximoContacto = modelo.ProximoContacto,
                    EtapaNegociacion = modelo.EtapaNegociacionId
                };

                var resultado = await _seguimientoServicio.RegistrarSeguimientoAsync(dto);
                return CreatedAtAction(nameof(GetByLead), new { leadId = resultado.LeadId }, resultado);
            }
            catch (UnauthorizedAccessException ex)
            {
                return Unauthorized(ex.Message);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(ex.Message);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ========== POST: Marcar recordatorio enviado ==========
        [HttpPost("{id}/recordatorio")]
        public async Task<IActionResult> MarcarRecordatorio(Guid id)
        {
            await _seguimientoServicio.MarcarRecordatorioEnviadoAsync(id);
            return NoContent();
        }
    }
}