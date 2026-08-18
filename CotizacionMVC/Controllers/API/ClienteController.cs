using CotizacionMVC.Servicios.Aplicacion.Dtos.Cliente;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CotizacionMVC.Controllers.API
{
    [Authorize]
    [ApiController]
    [Route("api/[controller]")]
    public class ClienteController : ControllerBase
    {
        private readonly IClienteServicio _clienteServicio;
        private readonly IUserContextService _userContextService;

        public ClienteController(
            IClienteServicio clienteServicio,
            IUserContextService userContextService)
        {
            _clienteServicio = clienteServicio;
            _userContextService = userContextService;
        }

        // ========== GET: Todos los clientes ==========
        [HttpGet]
        public async Task<ActionResult<IReadOnlyList<ClienteResumenDto>>> Get()
        {
            var usuarioId = await _userContextService.GetCurrentUserIdAsync();
            var clientes = await _clienteServicio.ObtenerTodosAsync(usuarioId);
            return Ok(clientes);
        }

        // ========== GET: Cliente por ID con sus cotizaciones ==========
        [HttpGet("{id}")]
        public async Task<ActionResult<ClienteDetalleDto>> Get(Guid id)
        {
            var cliente = await _clienteServicio.ObtenerPorIdAsync(id);
            if (cliente == null)
                return NotFound($"No se encontró el cliente con ID {id}");
            return Ok(cliente);
        }

        // ========== POST: Crear cliente ==========
        [HttpPost]
        public async Task<ActionResult<ClienteDetalleDto>> Post([FromBody] CrearClienteDto dto)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                var resultado = await _clienteServicio.CrearAsync(dto);
                return CreatedAtAction(nameof(Get), new { id = resultado.Id }, resultado);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ========== PUT: Actualizar cliente ==========
        [HttpPut("{id}")]
        public async Task<IActionResult> Put(Guid id, [FromBody] ActualizarClienteDto dto)
        {
            if (id != dto.Id)
                return BadRequest("El ID del cliente no coincide");

            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                await _clienteServicio.ActualizarAsync(dto);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"No se encontró el cliente con ID {id}");
            }
            catch (ArgumentException ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ========== DELETE: Eliminar cliente ==========
        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var resultado = await _clienteServicio.EliminarAsync(id);
                if (!resultado.Exitoso)
                    return BadRequest(resultado.MotivoFallo);
                return NoContent();
            }
            catch (KeyNotFoundException)
            {
                return NotFound($"No se encontró el cliente con ID {id}");
            }
        }
    }
}