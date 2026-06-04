using CotizacionMVC.Data;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Models.Valor;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Controllers
{
    public class ClienteController : Controller
    {
        private readonly ApplicationDbContext _contextoBaseDatos;

        public ClienteController(ApplicationDbContext contextoBaseDatos)
        {
            _contextoBaseDatos = contextoBaseDatos;
        }

        // GET: Cliente/Indice
        public async Task<IActionResult> Indice(string termino = null)
        {
            var consulta = _contextoBaseDatos.Clientes
                .Include(c => c.Cotizaciones)
                .AsQueryable();

            if (!string.IsNullOrWhiteSpace(termino))
            {
                termino = termino.ToLower();
                consulta = consulta.Where(c =>
                    c.Nombre.ToLower().Contains(termino) ||
                    c.Contacto.Telefono.ToLower().Contains(termino) ||
                    c.Contacto.TelefonoMovil.ToLower().Contains(termino) ||
                    c.Contacto.Correo.ToLower().Contains(termino)
                );
            }

            var clientes = await consulta
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ViewBag.TerminoBusqueda = termino;
            return View(clientes);
        }

        // GET: Cliente/Detalles/5
        public async Task<IActionResult> Detalles(Guid? id)
        {
            if (id == null)
            {
                return NotFound("No se proporcionó un identificador de cliente");
            }

            var cliente = await _contextoBaseDatos.Clientes
                .Include(c => c.Cotizaciones)
                .ThenInclude(cot => cot.Empresa)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound($"No se encontró el cliente con ID {id}");
            }

            return View(cliente);
        }

        // GET: Cliente/Crear
        public IActionResult Crear()
        {
            return View();
        }

        // POST: Cliente/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            string nombre,
            string telefono,
            string telefonoMovil,
            string correo,
            string nombreContacto,
            string calle,
            string numeroExterior,
            string numeroInterior,
            string colonia,
            string ciudad,
            string estado,
            string codigoPostal,
            string observaciones)
        {
            bool hayErrores = false;

            // Validar nombre obligatorio
            if (string.IsNullOrWhiteSpace(nombre))
            {
                ModelState.AddModelError("nombre", "El nombre del cliente es obligatorio");
                hayErrores = true;
            }

            // Validar que tenga al menos un medio de contacto
            bool tieneTelefono = !string.IsNullOrWhiteSpace(telefono);
            bool tieneMovil = !string.IsNullOrWhiteSpace(telefonoMovil);
            bool tieneCorreo = !string.IsNullOrWhiteSpace(correo);

            if (!tieneTelefono && !tieneMovil && !tieneCorreo)
            {
                ModelState.AddModelError("", "El cliente debe tener al menos un medio de contacto (teléfono, teléfono móvil o correo electrónico)");
                hayErrores = true;
            }

            if (hayErrores)
            {
                return View();
            }

            try
            {
                // Crear Value Object Contacto
                var contacto = new Contacto(telefono, telefonoMovil, correo, nombreContacto);

                // Crear cliente
                var cliente = new Cliente(nombre, contacto);

                // Agregar dirección si se proporcionó al menos un campo
                bool tieneDireccion = !string.IsNullOrWhiteSpace(calle) ||
                                      !string.IsNullOrWhiteSpace(colonia) ||
                                      !string.IsNullOrWhiteSpace(ciudad) ||
                                      !string.IsNullOrWhiteSpace(codigoPostal);

                if (tieneDireccion)
                {
                    var direccion = new Direccion(
                        calle ?? "",
                        numeroExterior,
                        colonia ?? "",
                        ciudad ?? "",
                        estado,
                        codigoPostal ?? "",
                        numeroInterior
                    );
                    cliente.ActualizarDireccion(direccion);
                }

                // Agregar observaciones
                if (!string.IsNullOrWhiteSpace(observaciones))
                {
                    cliente.AgregarObservaciones(observaciones);
                }

                _contextoBaseDatos.Clientes.Add(cliente);
                await _contextoBaseDatos.SaveChangesAsync();

                TempData["MensajeExito"] = $"Cliente {cliente.Nombre} creado exitosamente";
                return RedirectToAction(nameof(Indice));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                return View();
            }
        }

        // GET: Cliente/Editar/5
        public async Task<IActionResult> Editar(Guid? id)
        {
            if (id == null)
            {
                return NotFound("No se proporcionó un identificador de cliente");
            }

            var cliente = await _contextoBaseDatos.Clientes.FindAsync(id);

            if (cliente == null)
            {
                return NotFound($"No se encontró el cliente con ID {id}");
            }

            return View(cliente);
        }

        // POST: Cliente/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(
            Guid id,
            string nombre,
            string telefono,
            string telefonoMovil,
            string correo,
            string nombreContacto,
            string calle,
            string numeroExterior,
            string numeroInterior,
            string colonia,
            string ciudad,
            string estado,
            string codigoPostal,
            string observaciones)
        {
            bool hayErrores = false;

            // Validar nombre obligatorio
            if (string.IsNullOrWhiteSpace(nombre))
            {
                ModelState.AddModelError("nombre", "El nombre del cliente es obligatorio");
                hayErrores = true;
            }

            // Validar que tenga al menos un medio de contacto
            bool tieneTelefono = !string.IsNullOrWhiteSpace(telefono);
            bool tieneMovil = !string.IsNullOrWhiteSpace(telefonoMovil);
            bool tieneCorreo = !string.IsNullOrWhiteSpace(correo);

            if (!tieneTelefono && !tieneMovil && !tieneCorreo)
            {
                ModelState.AddModelError("", "El cliente debe tener al menos un medio de contacto (teléfono, teléfono móvil o correo electrónico)");
                hayErrores = true;
            }

            if (hayErrores)
            {
                var clienteOriginal = await _contextoBaseDatos.Clientes.FindAsync(id);
                return View(clienteOriginal);
            }

            try
            {
                var cliente = await _contextoBaseDatos.Clientes.FindAsync(id);

                if (cliente == null)
                {
                    return NotFound($"No se encontró el cliente con ID {id}");
                }

                // Actualizar contacto
                var nuevoContacto = new Contacto(telefono, telefonoMovil, correo, nombreContacto);
                cliente.ActualizarContacto(nuevoContacto);

                // Actualizar dirección
                bool tieneDireccion = !string.IsNullOrWhiteSpace(calle) ||
                                      !string.IsNullOrWhiteSpace(colonia) ||
                                      !string.IsNullOrWhiteSpace(ciudad) ||
                                      !string.IsNullOrWhiteSpace(codigoPostal);

                if (tieneDireccion)
                {
                    var direccion = new Direccion(
                        calle ?? "",
                        numeroExterior,
                        colonia ?? "",
                        ciudad ?? "",
                        estado,
                        codigoPostal ?? "",
                        numeroInterior
                    );
                    cliente.ActualizarDireccion(direccion);
                }
                else
                {
                    cliente.ActualizarDireccion(null);
                }

                // Actualizar observaciones
                cliente.AgregarObservaciones(observaciones);

                await _contextoBaseDatos.SaveChangesAsync();

                TempData["MensajeExito"] = $"Cliente {cliente.Nombre} actualizado exitosamente";
                return RedirectToAction(nameof(Indice));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                var clienteOriginal = await _contextoBaseDatos.Clientes.FindAsync(id);
                return View(clienteOriginal);
            }
        }

        // GET: Cliente/Eliminar/5 (Solo administrador)
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> Eliminar(Guid? id)
        {
            if (id == null)
            {
                return NotFound("No se proporcionó un identificador de cliente");
            }

            var cliente = await _contextoBaseDatos.Clientes
                .Include(c => c.Cotizaciones)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound($"No se encontró el cliente con ID {id}");
            }

            return View(cliente);
        }

        // POST: Cliente/Eliminar/5 (Solo administrador)
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        [Authorize(Roles = "Administrador")]
        public async Task<IActionResult> EliminarConfirmado(Guid id)
        {
            var cliente = await _contextoBaseDatos.Clientes
                .Include(c => c.Cotizaciones)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
            {
                return NotFound($"No se encontró el cliente con ID {id}");
            }

            if (cliente.Cotizaciones.Any())
            {
                TempData["MensajeError"] = $"No se puede eliminar el cliente {cliente.Nombre} porque tiene cotizaciones asociadas";
                return RedirectToAction(nameof(Indice));
            }

            _contextoBaseDatos.Clientes.Remove(cliente);
            await _contextoBaseDatos.SaveChangesAsync();

            TempData["MensajeExito"] = $"Cliente {cliente.Nombre} eliminado exitosamente";
            return RedirectToAction(nameof(Indice));
        }
    }
}