using CotizacionMVC.Models.Enums;
using CotizacionMVC.Servicios.Aplicacion.Dtos.Equipo;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using CotizacionMVC.ViewModels.Equipo;
using Microsoft.AspNetCore.Mvc;

namespace CotizacionMVC.Controllers
{
    public class EquipoController : Controller
    {
        private readonly IEquipoServicio _equipoServicio;

        public EquipoController(IEquipoServicio equipoServicio)
        {
            _equipoServicio = equipoServicio;
        }

        // GET: Equipo/Indice
        public async Task<IActionResult> Indice()
        {
            var equipos = await _equipoServicio.ObtenerTodosAsync();

            var viewModel = new EquipoIndiceViewModel
            {
                Equipos = equipos.Select(e => new EquipoResumenViewModel
                {
                    Id = e.Id,
                    Marca = e.Marca,
                    Modelo = e.Modelo,
                    CapacidadToneladas = e.CapacidadToneladas,
                    PrecioBase = e.PrecioBase,
                    MonedaOriginal = e.MonedaOriginal,
                    Activo = e.Activo
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: Equipo/Detalles/5
        public async Task<IActionResult> Detalles(Guid? id)
        {
            if (id == null)
                return NotFound("No se proporcionó un identificador de equipo");

            var equipo = await _equipoServicio.ObtenerPorIdAsync(id.Value);

            if (equipo == null)
                return NotFound($"No se encontró el equipo con ID {id}");

            var viewModel = new EquipoDetalleViewModel
            {
                Id = equipo.Id,
                Marca = equipo.Marca,
                Modelo = equipo.Modelo,
                Tipo = equipo.Tipo,
                CapacidadToneladas = equipo.CapacidadToneladas,
                Tension = equipo.Tension,
                Tecnologia = equipo.Tecnologia,
                PrecioBase = equipo.PrecioBase,
                MonedaOriginal = equipo.MonedaOriginal,
                Activo = equipo.Activo,
                FechaCreacion = equipo.FechaCreacion
            };

            return View(viewModel);
        }

        // GET: Equipo/Crear
        public IActionResult Crear()
        {
            var viewModel = new EquipoFormViewModel
            {
                MonedaOriginal = "USD"
            };

            ViewBag.Marcas = ObtenerListaMarcas();
            return View(viewModel);
        }

        // POST: Equipo/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(EquipoFormViewModel formulario)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Marcas = ObtenerListaMarcas();
                return View(formulario);
            }

            try
            {
                var dto = new CrearEquipoDto
                {
                    Marca = formulario.Marca,
                    Modelo = formulario.Modelo,
                    Tipo = formulario.Tipo,
                    CapacidadToneladas = formulario.CapacidadToneladas,
                    Tension = formulario.Tension,
                    Tecnologia = formulario.Tecnologia,
                    PrecioBase = formulario.PrecioBase,
                    MonedaOriginal = formulario.MonedaOriginal
                };

                var resultado = await _equipoServicio.CrearAsync(dto);

                TempData["MensajeExito"] = $"Equipo {resultado.Modelo} creado exitosamente";
                return RedirectToAction(nameof(Indice));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                ViewBag.Marcas = ObtenerListaMarcas();
                return View(formulario);
            }
        }

        // GET: Equipo/Editar/5
        public async Task<IActionResult> Editar(Guid? id)
        {
            if (id == null)
                return NotFound("No se proporcionó un identificador de equipo");

            var equipo = await _equipoServicio.ObtenerPorIdAsync(id.Value);

            if (equipo == null)
                return NotFound($"No se encontró el equipo con ID {id}");

            var viewModel = new EquipoFormViewModel
            {
                Id = equipo.Id,
                Marca = equipo.Marca,
                Modelo = equipo.Modelo,
                Tipo = equipo.Tipo,
                CapacidadToneladas = equipo.CapacidadToneladas,
                Tension = equipo.Tension,
                Tecnologia = equipo.Tecnologia,
                PrecioBase = equipo.PrecioBase,
                MonedaOriginal = equipo.MonedaOriginal
            };

            ViewBag.Marcas = ObtenerListaMarcas();
            return View(viewModel);
        }

        // POST: Equipo/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EquipoFormViewModel formulario)
        {
            if (!ModelState.IsValid)
            {
                ViewBag.Marcas = ObtenerListaMarcas();
                return View(formulario);
            }

            try
            {
                var dto = new ActualizarEquipoDto
                {
                    Id = formulario.Id.GetValueOrDefault(),
                    PrecioBase = formulario.PrecioBase
                };

                await _equipoServicio.ActualizarAsync(dto);

                TempData["MensajeExito"] = $"Equipo actualizado exitosamente";
                return RedirectToAction(nameof(Indice));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                ViewBag.Marcas = ObtenerListaMarcas();
                return View(formulario);
            }
        }

        // GET: Equipo/Eliminar/5
        public async Task<IActionResult> Eliminar(Guid? id)
        {
            if (id == null)
                return NotFound("No se proporcionó un identificador de equipo");

            var equipo = await _equipoServicio.ObtenerPorIdAsync(id.Value);

            if (equipo == null)
                return NotFound($"No se encontró el equipo con ID {id}");

            var viewModel = new EquipoDetalleViewModel
            {
                Id = equipo.Id,
                Marca = equipo.Marca,
                Modelo = equipo.Modelo,
                Tipo = equipo.Tipo,
                CapacidadToneladas = equipo.CapacidadToneladas,
                Tension = equipo.Tension,
                Tecnologia = equipo.Tecnologia,
                PrecioBase = equipo.PrecioBase,
                MonedaOriginal = equipo.MonedaOriginal,
                Activo = equipo.Activo,
                FechaCreacion = equipo.FechaCreacion
            };

            return View(viewModel);
        }

        // POST: Equipo/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(Guid id)
        {
            try
            {
                var resultado = await _equipoServicio.EliminarAsync(id);

                if (resultado.Desactivado)
                    TempData["MensajeAdvertencia"] = resultado.Mensaje;
                else
                    TempData["MensajeExito"] = resultado.Mensaje;

                return RedirectToAction(nameof(Indice));
            }
            catch (KeyNotFoundException ex)
            {
                return NotFound(ex.Message);
            }
        }

        private List<object> ObtenerListaMarcas()
        {
            return Enum.GetValues(typeof(TipoMarca))
                .Cast<TipoMarca>()
                .Select(m => new { Valor = (int)m, Nombre = m.ToString() })
                .ToList<object>();
        }
    }
}