using CotizacionMVC.Data;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Controllers
{
    public class CotizacionController : Controller
    {
        private readonly ApplicationDbContext _contextoBaseDatos;

        public CotizacionController(ApplicationDbContext contextoBaseDatos)
        {
            _contextoBaseDatos = contextoBaseDatos;
        }

        // GET: Cotizacion/Indice
        public async Task<IActionResult> Indice()
        {
            var cotizaciones = await _contextoBaseDatos.Cotizaciones
                .Include(c => c.Cliente)
                .Include(c => c.Empresa)
                .Include(c => c.Vendedor)
                .OrderByDescending(c => c.FechaCreacion)
                .ToListAsync();

            return View(cotizaciones);
        }

        // GET: Cotizacion/Detalles/5
        public async Task<IActionResult> Detalles(Guid? id)
        {
            if (id == null)
            {
                return NotFound("No se proporcionó un identificador de cotización");
            }

            var cotizacion = await _contextoBaseDatos.Cotizaciones
                .Include(c => c.Cliente)
                .Include(c => c.Empresa)
                .Include(c => c.Vendedor)
                .Include(c => c.ItemsEquipos)
                    .ThenInclude(i => i.Equipo)
                .Include(c => c.ItemsInstalacion)
                    .ThenInclude(i => i.Instalacion)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cotizacion == null)
            {
                return NotFound($"No se encontró la cotización con ID {id}");
            }

            return View(cotizacion);
        }

        // GET: Cotizacion/Crear
        public async Task<IActionResult> Crear()
        {
            ViewBag.Clientes = await _contextoBaseDatos.Clientes
                .OrderBy(c => c.Nombre)
                .ToListAsync();

            ViewBag.Equipos = await _contextoBaseDatos.Equipos
                .Where(e => e.Activo)
                .OrderBy(e => e.Marca)
                .ToListAsync();

            ViewBag.Instalaciones = await _contextoBaseDatos.Instalaciones
                .Where(i => i.Activo)
                .ToListAsync();

            return View();
        }

        // POST: Cotizacion/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(
            Guid clienteId,
            decimal areaMetrosCuadrados,
            string condicionesPago,
            string equipos,
            string instalaciones)
        {
            // Deserializar JSON
            var listaEquipos = string.IsNullOrEmpty(equipos)
                ? new List<ItemCotizacionJson>()
                : System.Text.Json.JsonSerializer.Deserialize<List<ItemCotizacionJson>>(equipos);

            var listaInstalaciones = string.IsNullOrEmpty(instalaciones)
                ? new List<ItemInstalacionJson>()
                : System.Text.Json.JsonSerializer.Deserialize<List<ItemInstalacionJson>>(instalaciones);

            // Validar cliente
            if (clienteId == Guid.Empty)
            {
                TempData["MensajeError"] = "Debe seleccionar un cliente";
                return RedirectToAction(nameof(Crear));
            }

            // Validar equipos
            if (listaEquipos == null || !listaEquipos.Any())
            {
                TempData["MensajeError"] = "Debe agregar al menos un equipo";
                return RedirectToAction(nameof(Crear));
            }

            // Obtener cliente
            var cliente = await _contextoBaseDatos.Clientes.FindAsync(clienteId);
            if (cliente == null)
            {
                TempData["MensajeError"] = "Cliente no encontrado";
                return RedirectToAction(nameof(Crear));
            }

            // Obtener empresa activa
            var empresa = await ObtenerEmpresaActual();
            if (empresa == null)
            {
                TempData["MensajeError"] = "No hay empresa activa";
                return RedirectToAction(nameof(Crear));
            }

            // Obtener vendedor actual
            var nombreVendedor = User.Identity?.Name ?? "Vendedor";
            var vendedor = await _contextoBaseDatos.Usuarios
                .FirstOrDefaultAsync(u => u.NombreCompleto == nombreVendedor);

            if (vendedor == null)
            {
                vendedor = new Usuario(nombreVendedor, nombreVendedor, RolUsuario.Vendedor);
                _contextoBaseDatos.Usuarios.Add(vendedor);
                await _contextoBaseDatos.SaveChangesAsync();
            }

            // Generar número de cotización
            var numeroCotizacion = await GenerarNumeroCotizacion();

            // Crear cotización
            var cotizacion = new Cotizacion(
                numeroCotizacion,
                cliente,
                empresa,
                vendedor,
                areaMetrosCuadrados,
                condicionesPago ?? ""
            );

            _contextoBaseDatos.Cotizaciones.Add(cotizacion);
            await _contextoBaseDatos.SaveChangesAsync();

            // Agregar equipos
            foreach (var eq in listaEquipos)
            {
                var equipo = await _contextoBaseDatos.Equipos.FindAsync(eq.EquipoId);
                if (equipo != null)
                {
                    var item = new ItemCotizacion(
                        cotizacion,
                        equipo,
                        eq.Cantidad,
                        empresa.UtilidadEmpresaPorcentaje,
                        empresa.UtilidadVendedorPorcentaje,
                        null
                    );
                    _contextoBaseDatos.ItemsCotizacion.Add(item);
                }
            }

          

            // Agregar instalaciones
            foreach (var inst in listaInstalaciones )             
            {
                var instalacion = inst.InstalacionId.HasValue
                    ? await _contextoBaseDatos.Instalaciones.FindAsync(inst.InstalacionId.Value)
                    : null;

                var itemInstalacion = new ItemInstalacion(
                    cotizacion,
                    inst.Concepto,
                    inst.Descripcion ?? "",
                    inst.Cantidad,
                    inst.CostoUnitario,
                    instalacion
                );
                _contextoBaseDatos.ItemsInstalacion.Add(itemInstalacion);
            }

            await _contextoBaseDatos.SaveChangesAsync();

            TempData["MensajeExito"] = $"Cotización {numeroCotizacion} creada exitosamente";
            return RedirectToAction(nameof(Detalles), new { id = cotizacion.Id });
        }

        // GET: Cotizacion/Editar/5
        public async Task<IActionResult> Editar(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cotizacion = await _contextoBaseDatos.Cotizaciones
                .Include(c => c.ItemsEquipos)
                .Include(c => c.ItemsInstalacion)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cotizacion == null)
            {
                return NotFound();
            }

            if (!cotizacion.PuedeSerModificada())
            {
                TempData["MensajeError"] = "Esta cotización no puede ser modificada porque ya está aceptada o cerrada";
                return RedirectToAction(nameof(Detalles), new { id });
            }

            ViewBag.Clientes = await _contextoBaseDatos.Clientes.OrderBy(c => c.Nombre).ToListAsync();
            ViewBag.Equipos = await _contextoBaseDatos.Equipos.Where(e => e.Activo).ToListAsync();

            return View(cotizacion);
        }

        // POST: Cotizacion/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Guid id, Guid clienteId, decimal areaMetrosCuadrados, string condicionesPago)
        {
            var cotizacion = await _contextoBaseDatos.Cotizaciones
                .Include(c => c.ItemsEquipos)
                .Include(c => c.ItemsInstalacion)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cotizacion == null)
            {
                return NotFound();
            }

            if (!cotizacion.PuedeSerModificada())
            {
                TempData["MensajeError"] = "Esta cotización no puede ser modificada";
                return RedirectToAction(nameof(Indice));
            }

            var cliente = await _contextoBaseDatos.Clientes.FindAsync(clienteId);
            if (cliente == null)
            {
                ModelState.AddModelError("", "Cliente no encontrado");
                return View(cotizacion);
            }

            // Actualizar datos básicos
            await _contextoBaseDatos.SaveChangesAsync();

            TempData["MensajeExito"] = $"Cotización {cotizacion.NumeroCotizacion} actualizada";
            return RedirectToAction(nameof(Detalles), new { id });
        }

        // GET: Cotizacion/Eliminar/5
        public async Task<IActionResult> Eliminar(Guid? id)
        {
            if (id == null)
            {
                return NotFound();
            }

            var cotizacion = await _contextoBaseDatos.Cotizaciones
                .Include(c => c.Cliente)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cotizacion == null)
            {
                return NotFound();
            }

            return View(cotizacion);
        }

        // POST: Cotizacion/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(Guid id)
        {
            var cotizacion = await _contextoBaseDatos.Cotizaciones
                .Include(c => c.ItemsEquipos)
                .Include(c => c.ItemsInstalacion)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cotizacion == null)
            {
                return NotFound();
            }

            _contextoBaseDatos.ItemsCotizacion.RemoveRange(cotizacion.ItemsEquipos);
            _contextoBaseDatos.ItemsInstalacion.RemoveRange(cotizacion.ItemsInstalacion);
            _contextoBaseDatos.Cotizaciones.Remove(cotizacion);

            await _contextoBaseDatos.SaveChangesAsync();

            TempData["MensajeExito"] = $"Cotización {cotizacion.NumeroCotizacion} eliminada";
            return RedirectToAction(nameof(Indice));
        }

        // GET: Cotizacion/CalcularCargaTermica
        [HttpGet]
        public IActionResult CalcularCargaTermica(decimal area)
        {
            var trSugerida = area / 16;
            var btuSugerida = trSugerida * 12000;

            return Json(new
            {
                tr = Math.Round(trSugerida, 1),
                btu = Math.Round(btuSugerida, 0)
            });
        }

        // POST: Cotizacion/CambiarEstado
        [HttpPost]
        public async Task<IActionResult> CambiarEstado(Guid cotizacionId, EstadoCotizacion nuevoEstado)
        {
            var cotizacion = await _contextoBaseDatos.Cotizaciones.FindAsync(cotizacionId);
            if (cotizacion == null)
            {
                return Json(new { success = false, message = "Cotización no encontrada" });
            }

            try
            {
                cotizacion.CambiarEstado(nuevoEstado);
                await _contextoBaseDatos.SaveChangesAsync();
                return Json(new { success = true, nuevoEstado = nuevoEstado.ToString() });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // ==================== MÉTODOS AUXILIARES PRIVADOS ====================

        private async Task<Empresa?> ObtenerEmpresaActual()
        {
            var empresaIdString = HttpContext.Session.GetString("EmpresaActivaId");
            if (string.IsNullOrEmpty(empresaIdString))
            {
                return await _contextoBaseDatos.Empresas.FirstOrDefaultAsync(e => e.Activa);
            }

            var empresaId = Guid.Parse(empresaIdString);
            return await _contextoBaseDatos.Empresas.FindAsync(empresaId);
        }

        private async Task<string> GenerarNumeroCotizacion()
        {
            string prefijo = "COT";
            var ultimaCotizacion = await _contextoBaseDatos.Cotizaciones
                .OrderByDescending(c => c.NumeroCotizacion)
                .FirstOrDefaultAsync();

            int numero = 1;
            if (ultimaCotizacion != null)
            {
                var partes = ultimaCotizacion.NumeroCotizacion.Split('-');
                if (partes.Length == 2 && int.TryParse(partes[1], out int ultimoNumero))
                {
                    numero = ultimoNumero + 1;
                }
            }

            return $"{prefijo}-{numero:D4}";
        }
    }

    // Clases para recibir JSON desde el formulario
    public class ItemCotizacionJson
    {
        public Guid EquipoId { get; set; }
        public int Cantidad { get; set; }
    }

    public class ItemInstalacionJson
    {
        public Guid? InstalacionId { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
    }
}