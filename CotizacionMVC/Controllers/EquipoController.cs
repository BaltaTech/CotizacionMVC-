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

        // ========== CATÁLOGO PARA VENDEDORES ==========

        [HttpGet]
        public async Task<IActionResult> Catalogo(TipoMarca? marca = null)
        {
            var marcas = Enum.GetValues(typeof(TipoMarca))
                .Cast<TipoMarca>()
                .Where(m => m != TipoMarca.Otro)
                .ToList();

            IReadOnlyList<string> sistemas;

            if (marca.HasValue)
            {
                sistemas = await _equipoServicio.ObtenerSistemasPorMarcaAsync(marca.Value);
            }
            else
            {
                sistemas = await _equipoServicio.ObtenerSistemasAsync();
            }

            ViewBag.Marcas = marcas;
            ViewBag.MarcaSeleccionada = marca;
            ViewBag.Sistemas = sistemas;

            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerModos(string sistema)
        {
            var modos = await _equipoServicio.ObtenerModosPorSistemaAsync(sistema);
            return Json(modos);
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerModelos(string sistema, string modo)
        {
            var equipos = await _equipoServicio.ObtenerPorSistemaYModoAsync(sistema, modo);
            var resultado = equipos.Select(e => new
            {
                e.Id,
                e.Marca,
                marcaNombre = e.Marca.ToString(),
                e.Modelo,
                e.Descripcion,
                e.CapacidadToneladas,
                e.PrecioBase,
                e.MonedaOriginal
            });
            return Json(resultado);
        }

        // ========== CRUD ADMIN ==========

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

        public async Task<IActionResult> Detalles(Guid? id)
        {
            if (id == null) return NotFound();
            var equipo = await _equipoServicio.ObtenerPorIdAsync(id.Value);
            if (equipo == null) return NotFound();
            return View(new EquipoDetalleViewModel
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
            });
        }

        public IActionResult Crear()
        {
            ViewBag.Marcas = ObtenerListaMarcas();
            return View(new EquipoFormViewModel { MonedaOriginal = "USD" });
        }

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
                ModelState.AddModelError("", $"Error: {ex.Message}");
                ViewBag.Marcas = ObtenerListaMarcas();
                return View(formulario);
            }
        }

        public async Task<IActionResult> Editar(Guid? id)
        {
            if (id == null) return NotFound();
            var equipo = await _equipoServicio.ObtenerPorIdAsync(id.Value);
            if (equipo == null) return NotFound();
            ViewBag.Marcas = ObtenerListaMarcas();
            return View(new EquipoFormViewModel
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
            });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(EquipoFormViewModel formulario)
        {
            if (!ModelState.IsValid) { ViewBag.Marcas = ObtenerListaMarcas(); return View(formulario); }
            try
            {
                await _equipoServicio.ActualizarAsync(new ActualizarEquipoDto
                {
                    Id = formulario.Id.GetValueOrDefault(),
                    PrecioBase = formulario.PrecioBase
                });
                TempData["MensajeExito"] = "Equipo actualizado";
                return RedirectToAction(nameof(Indice));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error: {ex.Message}");
                ViewBag.Marcas = ObtenerListaMarcas();
                return View(formulario);
            }
        }

        public async Task<IActionResult> Eliminar(Guid? id)
        {
            if (id == null) return NotFound();
            var equipo = await _equipoServicio.ObtenerPorIdAsync(id.Value);
            if (equipo == null) return NotFound();
            return View(new EquipoDetalleViewModel
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
            });
        }

        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(Guid id)
        {
            try
            {
                var resultado = await _equipoServicio.EliminarAsync(id);
                TempData[resultado.Desactivado ? "MensajeAdvertencia" : "MensajeExito"] = resultado.Mensaje;
                return RedirectToAction(nameof(Indice));
            }
            catch (KeyNotFoundException ex) { return NotFound(ex.Message); }
        }

        private List<object> ObtenerListaMarcas()
        {
            return Enum.GetValues(typeof(TipoMarca))
                .Cast<TipoMarca>()
                .Select(m => new { Valor = (int)m, Nombre = m.ToString() })
                .ToList<object>();
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerSistemasPorMarca(int marca)
        {
            var sistemas = await _equipoServicio.ObtenerSistemasPorMarcaAsync((TipoMarca)marca);
            return Json(sistemas);
        }
    }
}