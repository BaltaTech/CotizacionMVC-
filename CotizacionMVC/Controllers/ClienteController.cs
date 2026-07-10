using CotizacionMVC.Servicios.Aplicacion.Dtos.Cliente;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using CotizacionMVC.ViewModels.Cliente;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CotizacionMVC.Controllers
{
    public class ClienteController : Controller
    {
        private readonly IClienteServicio _clienteServicio;

        public ClienteController(IClienteServicio clienteServicio)
        {
            _clienteServicio = clienteServicio;
        }

        // GET: Cliente/Indice
        public async Task<IActionResult> Indice(string? termino = null)
        {
            var clientes = await _clienteServicio.ObtenerTodosAsync(termino);

            var viewModel = new ClienteIndiceViewModel
            {
                Clientes = clientes.Select(c => new ClienteResumenViewModel
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Telefono = c.Telefono,
                    Correo = c.Correo,
                    Estado = c.Estado,
                    Empresa = c.Empresa,
                    FechaRegistro = c.FechaRegistro,
                    CantidadCotizaciones = c.CantidadCotizaciones
                }).ToList(),
                TerminoBusqueda = termino
            };

            return View(viewModel);
        }

        // GET: Cliente/Detalles/5
        public async Task<IActionResult> Detalles(Guid? id)
        {
            if (id == null)
                return NotFound("No se proporcionó un identificador de cliente");

            var cliente = await _clienteServicio.ObtenerPorIdAsync(id.Value);

            if (cliente == null)
                return NotFound($"No se encontró el cliente con ID {id}");

            var viewModel = MapearADetalleViewModel(cliente);
            return View(viewModel);
        }

        // GET: Cliente/Crear
        public IActionResult Crear()
        {
            return View(new ClienteFormViewModel());
        }

        // POST: Cliente/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(ClienteFormViewModel formulario)
        {
            if (!ModelState.IsValid)
                return View(formulario);

            try
            {
                var dto = new CrearClienteDto
                {
                    Nombre = formulario.Nombre,
                    Telefono = formulario.Telefono,
                    TelefonoMovil = formulario.TelefonoMovil,
                    Correo = formulario.Correo,
                    NombreContacto = formulario.NombreContacto,
                    Calle = formulario.Calle,
                    NumeroExterior = formulario.NumeroExterior,
                    NumeroInterior = formulario.NumeroInterior,
                    Colonia = formulario.Colonia,
                    Ciudad = formulario.Ciudad,
                    Estado = formulario.Estado,
                    CodigoPostal = formulario.CodigoPostal,
                    Observaciones = formulario.Observaciones
                };

                var resultado = await _clienteServicio.CrearAsync(dto);

                TempData["MensajeExito"] = $"Cliente {resultado.Nombre} creado exitosamente";
                return RedirectToAction(nameof(Indice));
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(formulario);
            }
        }

        // GET: Cliente/Editar/5
        public async Task<IActionResult> Editar(Guid? id)
        {
            if (id == null)
                return NotFound("No se proporcionó un identificador de cliente");

            var cliente = await _clienteServicio.ObtenerParaEdicionAsync(id.Value);

            if (cliente == null)
                return NotFound($"No se encontró el cliente con ID {id}");

            var viewModel = new ClienteFormViewModel
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Telefono = cliente.Telefono,
                TelefonoMovil = cliente.TelefonoMovil,
                Correo = cliente.Correo,
                NombreContacto = cliente.NombreContacto,
                Calle = cliente.Calle,
                NumeroExterior = cliente.NumeroExterior,
                NumeroInterior = cliente.NumeroInterior,
                Colonia = cliente.Colonia,
                Ciudad = cliente.Ciudad,
                Estado = cliente.Estado,
                CodigoPostal = cliente.CodigoPostal,
                Observaciones = cliente.Observaciones
            };

            return View(viewModel);
        }

        // POST: Cliente/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(ClienteFormViewModel formulario)
        {
            if (!ModelState.IsValid)
                return View(formulario);

            try
            {
                var dto = new ActualizarClienteDto
                {
                    Id = formulario.Id.GetValueOrDefault(),
                    Nombre = formulario.Nombre,
                    Telefono = formulario.Telefono,
                    TelefonoMovil = formulario.TelefonoMovil,
                    Correo = formulario.Correo,
                    NombreContacto = formulario.NombreContacto,
                    Calle = formulario.Calle,
                    NumeroExterior = formulario.NumeroExterior,
                    NumeroInterior = formulario.NumeroInterior,
                    Colonia = formulario.Colonia,
                    Ciudad = formulario.Ciudad,
                    Estado = formulario.Estado,
                    CodigoPostal = formulario.CodigoPostal,
                    Observaciones = formulario.Observaciones
                };

                var resultado = await _clienteServicio.ActualizarAsync(dto);

                TempData["MensajeExito"] = $"Cliente {resultado.Nombre} actualizado exitosamente";
                return RedirectToAction(nameof(Indice));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (ArgumentException ex)
            {
                ModelState.AddModelError("", ex.Message);
                return View(formulario);
            }
        }

        // GET: Cliente/Eliminar/5 (Solo administrador)
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(Guid? id)
        {
            if (id == null)
                return NotFound("No se proporcionó un identificador de cliente");

            var cliente = await _clienteServicio.ObtenerParaEliminacionAsync(id.Value);

            if (cliente == null)
                return NotFound($"No se encontró el cliente con ID {id}");

            var viewModel = MapearADetalleViewModel(cliente);
            return View(viewModel);
        }

        // POST: Cliente/Eliminar/5 (Solo administrador)
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarConfirmado(Guid id)
        {
            try
            {
                var resultado = await _clienteServicio.EliminarAsync(id);

                if (!resultado.Exitoso)
                {
                    TempData["MensajeError"] = resultado.MotivoFallo;
                    return RedirectToAction(nameof(Indice));
                }

                TempData["MensajeExito"] = "Cliente eliminado exitosamente";
                return RedirectToAction(nameof(Indice));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        private ClienteDetalleViewModel MapearADetalleViewModel(ClienteDetalleDto dto)
        {
            return new ClienteDetalleViewModel
            {
                Id = dto.Id,
                Nombre = dto.Nombre,
                Telefono = dto.Telefono,
                TelefonoMovil = dto.TelefonoMovil,
                Correo = dto.Correo,
                NombreContacto = dto.NombreContacto,
                Calle = dto.Calle,
                NumeroExterior = dto.NumeroExterior,
                NumeroInterior = dto.NumeroInterior,
                Colonia = dto.Colonia,
                Ciudad = dto.Ciudad,
                Estado = dto.Estado,
                CodigoPostal = dto.CodigoPostal,
                Observaciones = dto.Observaciones,
                EstadoCliente = dto.EstadoCliente,
                FechaCreacion = dto.FechaCreacion
            };
        }
    }
}