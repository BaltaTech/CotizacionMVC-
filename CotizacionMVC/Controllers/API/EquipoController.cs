using CotizacionMVC.Models.Enums;
using CotizacionMVC.Servicios.Aplicacion.Dtos.Equipo;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CotizacionMVC.Controllers.API
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class EquipoController : ControllerBase
    {
        private readonly IEquipoServicio _equipoServicio;

        public EquipoController(IEquipoServicio equipoServicio)
        {
            _equipoServicio = equipoServicio;
        }

        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<EquipoResumenDto>>> Get()
        {
            var equipos = await _equipoServicio.ObtenerTodosAsync();
            return Ok(equipos);
        }

        [HttpGet("{id}")]
        public async Task<ActionResult<EquipoDetalleDto>> Get(Guid id)
        {
            var equipo = await _equipoServicio.ObtenerPorIdAsync(id);
            if (equipo == null)
                return NotFound($"No se encontró el equipo con ID {id}");
            return Ok(equipo);
        }

        [HttpGet("sistemas")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetSistemas([FromQuery] TipoMarca marca)
        {
            var sistemas = await _equipoServicio.ObtenerSistemasPorMarcaAsync(marca);
            return Ok(sistemas);
        }

        [HttpGet("modos")]
        public async Task<ActionResult<IReadOnlyList<string>>> GetModos([FromQuery] string sistema)
        {
            if (string.IsNullOrWhiteSpace(sistema))
                return BadRequest("El sistema es obligatorio");

            var modos = await _equipoServicio.ObtenerModosPorSistemaAsync(sistema);
            return Ok(modos);
        }

        [HttpGet("buscar")]
        public async Task<ActionResult<IReadOnlyList<EquipoResumenDto>>> GetEquipos(
            [FromQuery] string sistema,
            [FromQuery] string modo)
        {
            if (string.IsNullOrWhiteSpace(sistema) || string.IsNullOrWhiteSpace(modo))
                return BadRequest("Sistema y modo son obligatorios");

            var equipos = await _equipoServicio.ObtenerPorSistemaYModoAsync(sistema, modo);
            return Ok(equipos);
        }
    }
}