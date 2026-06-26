using CotizacionMVC.Data;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Models.Enums;
using CotizacionMVC.Servicios;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Controllers
{
    [Authorize] 
    public class CotizacionController : Controller
    {
        private readonly ApplicationDbContext _contextoBaseDatos;
        private readonly IDocumento _documentoService;
        private readonly UserManager<Usuario> _userManager;

        // Constructor actualizado
        public CotizacionController(
            ApplicationDbContext contextoBaseDatos,
            IDocumento documentoService,
            UserManager<Usuario> userManager)
        {
            _contextoBaseDatos = contextoBaseDatos;
            _documentoService = documentoService;
            _userManager = userManager;
        }

        // GET: Cotizacion/Indice
        public async Task<IActionResult> Indice()
        {
            var usuarioActual = await _userManager.GetUserAsync(User);

            IQueryable<Cotizacion> query = _contextoBaseDatos.Cotizaciones
                .Include(c => c.Cliente)
                .Include(c => c.Empresa)
                .Include(c => c.Vendedor);

            // Si NO es Administrador, solo ve sus cotizaciones
            if (!User.IsInRole("Administrador"))
            {
                query = query.Where(c => c.VendedorId == usuarioActual!.Id);
            }

            var cotizaciones = await query
                .OrderByDescending(c => c.FechaCreacion)
                .ToListAsync();

            return View(cotizaciones);
        }

        // GET: Cotizacion/Detalles/5
        public async Task<IActionResult> Detalles(Guid? id)
        {
            if (id == null)
                return NotFound("No se proporcionó un identificador de cotización");

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
                return NotFound($"No se encontró la cotización con ID {id}");

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
            var opcionesJson = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var listaEquipos = string.IsNullOrEmpty(equipos)
                ? new List<ItemCotizacionJson>()
                : System.Text.Json.JsonSerializer.Deserialize<List<ItemCotizacionJson>>(equipos, opcionesJson);

            var listaInstalaciones = string.IsNullOrEmpty(instalaciones)
                ? new List<ItemInstalacionJson>()
                : System.Text.Json.JsonSerializer.Deserialize<List<ItemInstalacionJson>>(instalaciones, opcionesJson);

            if (clienteId == Guid.Empty)
            {
                TempData["MensajeError"] = "Debe seleccionar un cliente";
                return RedirectToAction(nameof(Crear));
            }

            if (listaEquipos == null || !listaEquipos.Any())
            {
                TempData["MensajeError"] = "Debe agregar al menos un equipo";
                return RedirectToAction(nameof(Crear));
            }

            var cliente = await _contextoBaseDatos.Clientes.FindAsync(clienteId);
            if (cliente == null)
            {
                TempData["MensajeError"] = "Cliente no encontrado";
                return RedirectToAction(nameof(Crear));
            }

            var empresa = await ObtenerEmpresaActual();
            if (empresa == null)
            {
                TempData["MensajeError"] = "No hay empresa activa";
                return RedirectToAction(nameof(Crear));
            }

            // Obtener el vendedor autenticado (el usuario actual)
            var vendedor = await _userManager.GetUserAsync(User);
            if (vendedor == null)
            {
                TempData["MensajeError"] = "Debe iniciar sesión para crear cotizaciones";
                return RedirectToAction("Login", "Autenticacion");
            }

            var numeroCotizacion = await GenerarNumeroCotizacion();

            Cotizacion cotizacion;
            try
            {
                cotizacion = new Cotizacion(
                    numeroCotizacion,
                    cliente,
                    empresa,
                    vendedor,
                    areaMetrosCuadrados,
                    condicionesPago ?? ""
                );
            }
            catch (ArgumentException ex)
            {
                TempData["MensajeError"] = ex.Message;
                return RedirectToAction(nameof(Crear));
            }

            _contextoBaseDatos.Cotizaciones.Add(cotizacion);

            foreach (var eq in listaEquipos)
            {
                var equipo = await _contextoBaseDatos.Equipos.FindAsync(eq.EquipoId);
                if (equipo == null) continue;

                try
                {
                    cotizacion.AgregarEquipo(
                        equipo,
                        eq.Cantidad,
                        empresa.UtilidadEmpresaPorcentaje,
                        empresa.UtilidadVendedorPorcentaje,
                        null
                    );
                }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
                {
                    TempData["MensajeError"] = ex.Message;
                    return RedirectToAction(nameof(Crear));
                }
            }

            foreach (var inst in listaInstalaciones)
            {
                try
                {
                    if (inst.InstalacionId.HasValue)
                    {
                        var instalacion = await _contextoBaseDatos.Instalaciones
                            .FindAsync(inst.InstalacionId.Value);

                        if (instalacion != null)
                            cotizacion.AgregarInstalacionPredefinida(instalacion, inst.Cantidad);
                    }
                    else
                    {
                        cotizacion.AgregarInstalacion(
                            inst.Concepto,
                            inst.Descripcion ?? "",
                            inst.Cantidad,
                            inst.CostoUnitario
                        );
                    }
                }
                catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
                {
                    TempData["MensajeError"] = ex.Message;
                    return RedirectToAction(nameof(Crear));
                }
            }

            var itemsCount = cotizacion.ItemsEquipos.Count;
            var instCount = cotizacion.ItemsInstalacion.Count;
            Console.WriteLine($"DEBUG CREAR: ItemsEquipos en memoria: {itemsCount}, ItemsInstalacion: {instCount}");
            TempData["DebugItems"] = $"Equipos: {itemsCount}, Instalaciones: {instCount}";

            // Persistencia normal en BD
            await _contextoBaseDatos.SaveChangesAsync();


            _contextoBaseDatos.Entry(cotizacion).State = EntityState.Detached;

            foreach (var item in cotizacion.ItemsEquipos)
            {
                _contextoBaseDatos.Entry(item).State = EntityState.Detached;
            }
            foreach (var inst in cotizacion.ItemsInstalacion)
            {
                _contextoBaseDatos.Entry(inst).State = EntityState.Detached;
            }

            TempData["MensajeExito"] = $"Cotización {numeroCotizacion} creada exitosamente";
            return RedirectToAction(nameof(Detalles), new { id = cotizacion.Id });
        }

        // GET: Cotizacion/DescargarPdf/5
        [HttpGet]
        public async Task<IActionResult> DescargarPdf(Guid id)
        {
            var cotizacion = await _contextoBaseDatos.Cotizaciones
                .AsNoTracking()
                .Include(c => c.Cliente)
                .Include(c => c.Empresa)
                .Include(c => c.Vendedor)
                .Include(c => c.ItemsEquipos)
                    .ThenInclude(i => i.Equipo)
                .Include(c => c.ItemsInstalacion)
                    .ThenInclude(i => i.Instalacion)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cotizacion == null)
                return NotFound("Cotización no encontrada");

            if (!string.IsNullOrEmpty(cotizacion.RutaPdf))
            {
                var rutaExistente = Path.Combine(Directory.GetCurrentDirectory(), cotizacion.RutaPdf);
                FileInfo fileInfo = new FileInfo(rutaExistente);

                if (fileInfo.Exists && fileInfo.Length > 0)
                {
                    var bytesExistentes = await System.IO.File.ReadAllBytesAsync(rutaExistente);
                    return File(bytesExistentes, _documentoService.TipoContenido,
                        $"{cotizacion.NumeroCotizacion}{_documentoService.ExtensionArchivo}");
                }
            }

            // Si llegó aquí, generará el PDF con los datos frescos del AsNoTracking()
            var pdfBytes = _documentoService.Generar(cotizacion);

            // Guardar archivo
            var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdf", "cotizaciones");
            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            var nombreArchivo = $"{cotizacion.NumeroCotizacion}{_documentoService.ExtensionArchivo}";
            var rutaCompleta = Path.Combine(carpeta, nombreArchivo);
            await System.IO.File.WriteAllBytesAsync(rutaCompleta, pdfBytes);

            // Guardar ruta en BD
            var rutaRelativa = $"wwwroot/pdf/cotizaciones/{nombreArchivo}";

            _contextoBaseDatos.Cotizaciones.Attach(cotizacion);
            cotizacion.GuardarRutaPdf(rutaRelativa);
            await _contextoBaseDatos.SaveChangesAsync();

            return File(pdfBytes, _documentoService.TipoContenido, nombreArchivo);
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
                return NotFound();

            if (!cotizacion.PuedeSerModificada())
            {
                TempData["MensajeError"] = "Esta cotización no puede ser modificada";
                return RedirectToAction(nameof(Indice));
            }

            var cliente = await _contextoBaseDatos.Clientes.FindAsync(clienteId);
            if (cliente == null)
            {
                TempData["MensajeError"] = "Cliente no encontrado";
                return RedirectToAction(nameof(Editar), new { id });
            }

            try
            {
                cotizacion.ActualizarDatosBasicos(cliente, areaMetrosCuadrados, condicionesPago ?? "");
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                TempData["MensajeError"] = ex.Message;
                return RedirectToAction(nameof(Editar), new { id });
            }

            await _contextoBaseDatos.SaveChangesAsync();

            TempData["MensajeExito"] = $"Cotización {cotizacion.NumeroCotizacion} actualizada";
            return RedirectToAction(nameof(Detalles), new { id });
        }

        // GET: Cotizacion/Eliminar/5
        public async Task<IActionResult> Eliminar(Guid? id)
        {
            if (id == null)
                return NotFound();

            var cotizacion = await _contextoBaseDatos.Cotizaciones
                .Include(c => c.Cliente)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cotizacion == null)
                return NotFound();

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
                return NotFound();

            if (!cotizacion.PuedeSerModificada())
            {
                TempData["MensajeError"] = "No se puede eliminar una cotización en este estado";
                return RedirectToAction(nameof(Indice));
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
                return Json(new { success = false, message = "Cotización no encontrada" });

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
                return await _contextoBaseDatos.Empresas.FirstOrDefaultAsync(e => e.Activa);

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
                    numero = ultimoNumero + 1;
            }

            return $"{prefijo}-{numero:D4}";
        }
    }

    // Estas clases pueden moverse a Models/ViewModels/
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