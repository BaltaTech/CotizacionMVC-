using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Models.Enums;
using CotizacionMVC.Servicios;
using CotizacionMVC.Servicios.Aplicacion;
using CotizacionMVC.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CotizacionMVC.Controllers
{
    [Authorize]
    public class CotizacionController : Controller
    {
        private readonly ICotizacionRepository _cotizacionRepo;
        private readonly IClienteRepository _clienteRepo;
        private readonly IEquipoRepository _equipoRepo;
        private readonly IInstalacionRepository _instalacionRepo;
        private readonly IEmpresaRepository _empresaRepo;
        private readonly IDocumento _documentoService;
        private readonly UserManager<Usuario> _userManager;
        private readonly CotizacionServicio _cotizacionServicio;

        public CotizacionController(
            ICotizacionRepository cotizacionRepo,
            IClienteRepository clienteRepo,
            IEquipoRepository equipoRepo,
            IInstalacionRepository instalacionRepo,
            IEmpresaRepository empresaRepo,
            IDocumento documentoService,
            UserManager<Usuario> userManager,
            CotizacionServicio cotizacionServicio)
        {
            _cotizacionRepo = cotizacionRepo;
            _clienteRepo = clienteRepo;
            _equipoRepo = equipoRepo;
            _instalacionRepo = instalacionRepo;
            _empresaRepo = empresaRepo;
            _documentoService = documentoService;
            _userManager = userManager;
            _cotizacionServicio = cotizacionServicio;
        }

        // GET: Cotizacion/Indice
        public async Task<IActionResult> Indice()
        {
            var usuarioActual = await _userManager.GetUserAsync(User);

            IEnumerable<Cotizacion> cotizaciones;

            if (User.IsInRole("Administrador"))
            {
                cotizaciones = await _cotizacionRepo.ObtenerTodasConRelacionesAsync();
            }
            else
            {
                cotizaciones = await _cotizacionRepo.ObtenerPorVendedorAsync(usuarioActual!.Id);
            }

            return View(cotizaciones);
        }

        // GET: Cotizacion/Detalles/5
        public async Task<IActionResult> Detalles(Guid? id)
        {
            if (id == null)
                return NotFound("No se proporcionó un identificador de cotización");

            var cotizacion = await _cotizacionRepo.ObtenerCompletaPorIdAsync(id.Value);

            if (cotizacion == null)
                return NotFound($"No se encontró la cotización con ID {id}");

            return View(cotizacion);
        }

        // GET: Cotizacion/Crear
        public async Task<IActionResult> Crear()
        {
            ViewBag.Clientes = await _clienteRepo.ObtenerTodosOrdenadosAsync();
            ViewBag.Equipos = await _equipoRepo.ObtenerTodosOrdenadosAsync();
            ViewBag.Instalaciones = await _instalacionRepo.ObtenerActivasAsync();

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
            // 1. Deserializar JSON
            var (listaEquipos, listaInstalaciones) = DeserializarItems(equipos, instalaciones);

            // 2. Obtener vendedor autenticado
            var vendedor = await _userManager.GetUserAsync(User);
            if (vendedor == null)
            {
                TempData["MensajeError"] = "Debe iniciar sesión para crear cotizaciones";
                return RedirectToAction("Login", "Autenticacion");
            }

            // 3. Obtener empresa actual
            var empresa = await ObtenerEmpresaActual();
            if (empresa == null)
            {
                TempData["MensajeError"] = "No hay empresa activa";
                return RedirectToAction(nameof(Crear));
            }

            // 4. Preparar solicitud para el caso de uso
            var solicitud = new SolicitudCrearCotizacion
            {
                ClienteId = clienteId,
                EmpresaId = empresa.Id,
                Vendedor = vendedor,
                AreaMetrosCuadrados = areaMetrosCuadrados,
                CondicionesPago = condicionesPago,
                Equipos = listaEquipos,
                Instalaciones = listaInstalaciones
            };

            // 5. Ejecutar caso de uso
            var resultado = await _cotizacionServicio.CrearCotizacionAsync(solicitud);

            // 6. Manejar resultado
            if (!resultado.Exitoso)
            {
                TempData["MensajeError"] = resultado.MensajeError;
                return RedirectToAction(nameof(Crear));
            }

            TempData["MensajeExito"] = $"Cotización {resultado.Cotizacion!.NumeroCotizacion} creada exitosamente";
            return RedirectToAction(nameof(Detalles), new { id = resultado.Cotizacion.Id });
        }

        // GET: Cotizacion/DescargarPdf/5
        [HttpGet]
        public async Task<IActionResult> DescargarPdf(Guid id)
        {
            var cotizacion = await _cotizacionRepo.ObtenerCompletaPorIdAsync(id);

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

            var pdfBytes = _documentoService.Generar(cotizacion);

            var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdf", "cotizaciones");
            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            var nombreArchivo = $"{cotizacion.NumeroCotizacion}{_documentoService.ExtensionArchivo}";
            var rutaCompleta = Path.Combine(carpeta, nombreArchivo);
            await System.IO.File.WriteAllBytesAsync(rutaCompleta, pdfBytes);

            var rutaRelativa = $"wwwroot/pdf/cotizaciones/{nombreArchivo}";
            cotizacion.GuardarRutaPdf(rutaRelativa);
            _cotizacionRepo.Update(cotizacion);

            return File(pdfBytes, _documentoService.TipoContenido, nombreArchivo);
        }

        // POST: Cotizacion/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Guid id, Guid clienteId, decimal areaMetrosCuadrados, string condicionesPago)
        {
            var cotizacion = await _cotizacionRepo.ObtenerConItemsAsync(id);

            if (cotizacion == null)
                return NotFound();

            if (!cotizacion.PuedeSerModificada())
            {
                TempData["MensajeError"] = "Esta cotización no puede ser modificada";
                return RedirectToAction(nameof(Indice));
            }

            var cliente = await _clienteRepo.GetByIdAsync(clienteId);
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

            _cotizacionRepo.Update(cotizacion);

            TempData["MensajeExito"] = $"Cotización {cotizacion.NumeroCotizacion} actualizada";
            return RedirectToAction(nameof(Detalles), new { id });
        }

        // GET: Cotizacion/Eliminar/5
        public async Task<IActionResult> Eliminar(Guid? id)
        {
            if (id == null)
                return NotFound();

            var cotizacion = await _cotizacionRepo.ObtenerConClienteAsync(id.Value);

            if (cotizacion == null)
                return NotFound();

            return View(cotizacion);
        }

        // POST: Cotizacion/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(Guid id)
        {
            var cotizacion = await _cotizacionRepo.ObtenerConItemsAsync(id);

            if (cotizacion == null)
                return NotFound();

            if (!cotizacion.PuedeSerModificada())
            {
                TempData["MensajeError"] = "No se puede eliminar una cotización en este estado";
                return RedirectToAction(nameof(Indice));
            }

            _cotizacionRepo.Delete(cotizacion);

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
            var cotizacion = await _cotizacionRepo.GetByIdAsync(cotizacionId);
            if (cotizacion == null)
                return Json(new { success = false, message = "Cotización no encontrada" });

            try
            {
                cotizacion.CambiarEstado(nuevoEstado);
                _cotizacionRepo.Update(cotizacion);
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
                return await _empresaRepo.ObtenerActivaAsync();

            var empresaId = Guid.Parse(empresaIdString);
            return await _empresaRepo.GetByIdAsync(empresaId);
        }

        private (List<ItemCotizacionJson>, List<ItemInstalacionJson>) DeserializarItems(
            string equipos, string instalaciones)
        {
            var opcionesJson = new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            };

            var listaEquipos = string.IsNullOrEmpty(equipos)
                ? new List<ItemCotizacionJson>()
                : System.Text.Json.JsonSerializer.Deserialize<List<ItemCotizacionJson>>(equipos, opcionesJson)
                  ?? new List<ItemCotizacionJson>();

            var listaInstalaciones = string.IsNullOrEmpty(instalaciones)
                ? new List<ItemInstalacionJson>()
                : System.Text.Json.JsonSerializer.Deserialize<List<ItemInstalacionJson>>(instalaciones, opcionesJson)
                  ?? new List<ItemInstalacionJson>();

            return (listaEquipos, listaInstalaciones);
        }
    }
}