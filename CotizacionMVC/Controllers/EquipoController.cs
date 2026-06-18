using CotizacionMVC.Data;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Controllers
{
    public class EquipoController : Controller
    {
        private readonly ApplicationDbContext _contextoBaseDatos;

        public EquipoController(ApplicationDbContext contextoBaseDatos)
        {
            _contextoBaseDatos = contextoBaseDatos;
        }

        // GET: Equipo/Indice
        public async Task<IActionResult> Indice()
        {
            var equipos = await _contextoBaseDatos.Equipos
                .Where(e => e.Activo)
                .OrderBy(e => e.Marca)
                .ThenBy(e => e.Modelo)
                .ToListAsync();

            return View(equipos);
        }

        // GET: Equipo/Detalles/5
        public async Task<IActionResult> Detalles(Guid? id)
        {
            if (id == null)
            {
                return NotFound("No se proporcionó un identificador de equipo");
            }

            var equipo = await _contextoBaseDatos.Equipos
                .FirstOrDefaultAsync(e => e.Id == id);

            if (equipo == null)
            {
                return NotFound($"No se encontró el equipo con ID {id}");
            }

            return View(equipo);
        }

        // GET: Equipo/Crear
        public IActionResult Crear()
        {
            ViewBag.Marcas = ObtenerListaMarcas();
            return View();
        }

        // POST: Equipo/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            TipoMarca Marca,
            string Modelo,
            string? Tipo,
            decimal CapacidadToneladas,
            string? Tension,
            string? Tecnologia,
            decimal PrecioBase,
            string MonedaOriginal)
        {
            bool hayErrores = false;

            // Validar campos obligatorios
            if (string.IsNullOrWhiteSpace(Modelo))
            {
                ModelState.AddModelError("Modelo", "El modelo es obligatorio");
                hayErrores = true;
            }

            if (PrecioBase <= 0)
            {
                ModelState.AddModelError("PrecioBase", "El precio base debe ser mayor a cero");
                hayErrores = true;
            }

            if (CapacidadToneladas <= 0)
            {
                ModelState.AddModelError("CapacidadToneladas", "La capacidad debe ser mayor a cero");
                hayErrores = true;
            }

            if (string.IsNullOrWhiteSpace(MonedaOriginal))
            {
                ModelState.AddModelError("MonedaOriginal", "La moneda es obligatoria");
                hayErrores = true;
            }

            // Validar reglas de negocio de moneda por marca
            if (!hayErrores)
            {
                if ((Marca == TipoMarca.Trane || Marca == TipoMarca.York) && MonedaOriginal != "USD")
                {
                    ModelState.AddModelError("MonedaOriginal", "Los equipos Trane y York solo pueden tener precio en USD");
                    hayErrores = true;
                }

                if (Marca != TipoMarca.Trane && Marca != TipoMarca.York && MonedaOriginal != "MXN")
                {
                    ModelState.AddModelError("MonedaOriginal", $"Los equipos {Marca} solo pueden tener precio en MXN");
                    hayErrores = true;
                }
            }

            if (hayErrores)
            {
                ViewBag.Marcas = ObtenerListaMarcas();
                return View();
            }

            try
            {
                var equipo = new Equipo(
                    Marca,
                    Modelo,
                    CapacidadToneladas,
                    PrecioBase,
                    MonedaOriginal
                );

                if (!string.IsNullOrWhiteSpace(Tipo))
                {
                    equipo.CompletarDetalles(Tipo, Tension ?? "", Tecnologia ?? "");
                }

                _contextoBaseDatos.Equipos.Add(equipo);
                await _contextoBaseDatos.SaveChangesAsync();

                TempData["MensajeExito"] = $"Equipo {equipo.Modelo} creado exitosamente";
                return RedirectToAction(nameof(Indice));
            }
            catch (Exception ex)
            {
                ModelState.AddModelError("", $"Error al guardar: {ex.Message}");
                ViewBag.Marcas = ObtenerListaMarcas();
                return View();
            }
        }

        // GET: Equipo/Editar/5
        public async Task<IActionResult> Editar(Guid? id)
        {
            if (id == null)
            {
                return NotFound("No se proporcionó un identificador de equipo");
            }

            var equipo = await _contextoBaseDatos.Equipos.FindAsync(id);

            if (equipo == null)
            {
                return NotFound($"No se encontró el equipo con ID {id}");
            }

            ViewBag.Marcas = ObtenerListaMarcas();
            return View(equipo);
        }

        // POST: Equipo/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Guid id, Equipo equipo)
        {
            if (id != equipo.Id)
            {
                return NotFound("El identificador del equipo no coincide");
            }

            if (string.IsNullOrWhiteSpace(equipo.Modelo))
            {
                ModelState.AddModelError("Modelo", "El modelo es obligatorio");
            }

            if (equipo.PrecioBase <= 0)
            {
                ModelState.AddModelError("PrecioBase", "El precio base debe ser mayor a cero");
            }

            if (equipo.CapacidadToneladas <= 0)
            {
                ModelState.AddModelError("CapacidadToneladas", "La capacidad debe ser mayor a cero");
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var equipoExistente = await _contextoBaseDatos.Equipos.FindAsync(id);

                    if (equipoExistente == null)
                    {
                        return NotFound($"No se encontró el equipo con ID {id}");
                    }

                    equipoExistente.ActualizarPrecio(equipo.PrecioBase);
                    _contextoBaseDatos.Entry(equipoExistente).CurrentValues.SetValues(equipo);

                    await _contextoBaseDatos.SaveChangesAsync();

                    TempData["MensajeExito"] = $"Equipo {equipoExistente.Modelo} actualizado exitosamente";
                    return RedirectToAction(nameof(Indice));
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!EquipoExiste(id))
                    {
                        return NotFound($"El equipo con ID {id} ya no existe");
                    }
                    throw;
                }
                catch (Exception ex)
                {
                    ModelState.AddModelError("", $"Error al actualizar: {ex.Message}");
                }
            }

            ViewBag.Marcas = ObtenerListaMarcas();
            return View(equipo);
        }

        // GET: Equipo/Eliminar/5
        public async Task<IActionResult> Eliminar(Guid? id)
        {
            if (id == null)
            {
                return NotFound("No se proporcionó un identificador de equipo");
            }

            var equipo = await _contextoBaseDatos.Equipos
                .FirstOrDefaultAsync(e => e.Id == id);

            if (equipo == null)
            {
                return NotFound($"No se encontró el equipo con ID {id}");
            }

            return View(equipo);
        }

        // POST: Equipo/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(Guid id)
        {
            var equipo = await _contextoBaseDatos.Equipos.FindAsync(id);

            if (equipo == null)
            {
                return NotFound($"No se encontró el equipo con ID {id}");
            }

            var estaEnUso = await _contextoBaseDatos.ItemsCotizacion
                .AnyAsync(i => i.EquipoId == id);

            if (estaEnUso)
            {
                equipo.Desactivar();
                TempData["MensajeAdvertencia"] = $"El equipo {equipo.Modelo} está en uso en cotizaciones. Se ha desactivado pero no eliminado.";
            }
            else
            {
                _contextoBaseDatos.Equipos.Remove(equipo);
                TempData["MensajeExito"] = $"Equipo {equipo.Modelo} eliminado exitosamente";
            }

            await _contextoBaseDatos.SaveChangesAsync();
            return RedirectToAction(nameof(Indice));
        }

        private bool EquipoExiste(Guid id)
        {
            return _contextoBaseDatos.Equipos.Any(e => e.Id == id);
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