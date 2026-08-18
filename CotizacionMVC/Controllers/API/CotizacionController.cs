using CotizacionMVC.Servicios.Aplicacion.Dtos.Cotizacion;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using CotizacionMVC.ViewModels;
using CotizacionMVC.ViewModels.Cotizacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CotizacionMVC.Controllers.API
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class CotizacionController : ControllerBase
    {
        private readonly ICotizacionServicio _cotizacionServicio;
        private readonly IUserContextService _userContextService;
        private readonly IEmpresaServicio _empresaServicio;

        public CotizacionController(
            ICotizacionServicio cotizacionServicio,
            IUserContextService userContextService,
            IEmpresaServicio empresaServicio)
        {
            _cotizacionServicio = cotizacionServicio;
            _userContextService = userContextService;
            _empresaServicio = empresaServicio;
        }

        [HttpGet]
        public async Task<ActionResult<IEnumerable<CotizacionResumenDto>>> Get()
        {
            var cotizaciones = await _cotizacionServicio.ObtenerIndiceAsync();
            return Ok(cotizaciones);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<CotizacionDetalleDto>> Get(Guid id)
        {
            var cotizacion = await _cotizacionServicio.ObtenerDetalleAsync(id);

            if (cotizacion == null)
                return NotFound($"No se encontró la cotización con ID {id}");

            return Ok(cotizacion);
        }

        [HttpPost]
        public async Task<ActionResult<CotizacionDetalleDto>> Post([FromBody] CrearCotizacionViewModel modelo)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            var empresaActiva = await _cotizacionServicio.ObtenerEmpresaActivaAsync();
            if (empresaActiva == null)
                return BadRequest("Debe seleccionar una empresa primero");

            if (!modelo.ClienteId.HasValue || modelo.ClienteId.Value == Guid.Empty)
                return BadRequest("Debe seleccionar un cliente");

            var equipos = DeserializarEquipos(modelo.EquiposJson);
            if (equipos == null || !equipos.Any())
                return BadRequest("Debe agregar al menos un equipo");

            var instalaciones = DeserializarInstalaciones(modelo.InstalacionesJson);

            var vendedor = await _userContextService.GetCurrentUserAsync();
            if (vendedor == null)
                return Unauthorized("Usuario no autenticado");

            var dto = new CrearCotizacionDto
            {
                ClienteId = modelo.ClienteId.Value,
                EmpresaId = empresaActiva.Id,
                VendedorId = vendedor.Id,
                AreaMetrosCuadrados = modelo.AreaMetrosCuadrados,
                CondicionesPago = modelo.CondicionesPago ?? string.Empty,
                Equipos = equipos,
                Instalaciones = instalaciones,
                LeadId = modelo.LeadId,
                TipoCambio = 17.43m,
                RecargoCiudadPorcentaje = 0
            };

            var resultado = await _cotizacionServicio.CrearAsync(dto);

            if (!resultado.Exitoso)
                return BadRequest(resultado.MensajeError);

            return CreatedAtAction(nameof(Get), new { id = resultado.Cotizacion!.Id }, resultado.Cotizacion);
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] ActualizarCotizacionDto dto)
        {
            if (id != dto.Id)
                return BadRequest("El ID de la cotización no coincide");

            var resultado = await _cotizacionServicio.ActualizarAsync(dto);

            if (!resultado.Exitoso)
                return BadRequest(resultado.MensajeError);

            return NoContent();
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            var resultado = await _cotizacionServicio.EliminarAsync(id);

            if (!resultado.Exitoso)
                return BadRequest(resultado.MensajeError);

            return NoContent();
        }


        private List<ItemCotizacionJson> DeserializarEquipos(string? json)
        {
            if (string.IsNullOrEmpty(json)) return new();
            return JsonSerializer.Deserialize<List<ItemCotizacionJson>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }

        private List<ItemInstalacionJson> DeserializarInstalaciones(string? json)
        {
            if (string.IsNullOrEmpty(json)) return new();
            return JsonSerializer.Deserialize<List<ItemInstalacionJson>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }
    }
}