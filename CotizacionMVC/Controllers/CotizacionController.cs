using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Models.Enums;
using CotizacionMVC.Servicios.Aplicacion.Dtos.Cotizacion;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using CotizacionMVC.ViewModels;
using CotizacionMVC.ViewModels.Cotizacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace CotizacionMVC.Controllers
{
    [Authorize]
    public class CotizacionController : Controller
    {
        private readonly ICotizacionServicio _cotizacionServicio;
        private readonly UserManager<Usuario> _userManager;
        private readonly IEmpresaRepository _empresaRepo;

        public CotizacionController(
            ICotizacionServicio cotizacionServicio,
            UserManager<Usuario> userManager,
            IEmpresaRepository empresaRepo)
        {
            _cotizacionServicio = cotizacionServicio;
            _userManager = userManager;
            _empresaRepo = empresaRepo;
        }

        // GET: Cotizacion/Indice
        public async Task<IActionResult> Indice()
        {
            var usuarioActual = await _userManager.GetUserAsync(User);
            var empresaIdString = HttpContext.Session.GetString("EmpresaActivaId");
            Guid? empresaId = string.IsNullOrEmpty(empresaIdString) ? null : Guid.Parse(empresaIdString);
            bool esAdmin = User.IsInRole("Administrador");

            var cotizaciones = await _cotizacionServicio.ObtenerIndiceAsync(
                usuarioActual?.Id, empresaId, esAdmin);

            var leads = await _cotizacionServicio.ObtenerLeadsDelVendedorAsync(usuarioActual!.Id);

            var viewModel = new CotizacionIndiceViewModel
            {
                Cotizaciones = cotizaciones.Select(c => new CotizacionResumenViewModel
                {
                    Id = c.Id,
                    NumeroCotizacion = c.NumeroCotizacion,
                    ClienteNombre = c.ClienteNombre,
                    EmpresaNombre = c.EmpresaNombre,
                    FechaCreacion = c.FechaCreacion,
                    Total = c.Total,
                    Moneda = c.Moneda,
                    Estado = c.Estado
                }).ToList(),
                Leads = leads.Select(l => new LeadResumenViewModel
                {
                    Id = l.Id,
                    ClienteNombre = l.ClienteNombre,
                    ClienteId = l.ClienteId,
                    Telefono = l.Telefono,
                    ProductoBusca = l.ProductoBusca,
                    EmpresaNombre = l.EmpresaNombre,
                    Estado = l.Estado,
                    FechaAsignacion = l.FechaAsignacion,
                    FechaCreacion = l.FechaCreacion,
                    NombreContacto = l.NombreContacto,
                    ClienteTelefono = l.ClienteTelefono,   
                    OrigenLead = l.OrigenLead
                }).ToList()
            };

            return View(viewModel);
        }

        // GET: Cotizacion/Detalles/5
        public async Task<IActionResult> Detalles(Guid? id)
        {
            if (id == null)
                return NotFound("No se proporcionó un identificador de cotización");

            var cotizacion = await _cotizacionServicio.ObtenerDetalleAsync(id.Value);

            if (cotizacion == null)
                return NotFound($"No se encontró la cotización con ID {id}");

            var viewModel = MapearADetalleViewModel(cotizacion);
            return View(viewModel);
        }

        // GET: Cotizacion/Crear
        public async Task<IActionResult> Crear(Guid? leadId = null)
        {
            var usuarioActual = await _userManager.GetUserAsync(User);
            bool esVendedor = User.IsInRole("Vendedor");

            var datos = await _cotizacionServicio.ObtenerDatosParaCrearAsync(
                usuarioActual!.Id, esVendedor, leadId);

            var viewModel = new CrearCotizacionViewModel();

            if (datos.Lead != null)
            {
                viewModel.LeadId = datos.Lead.Id;
                viewModel.ClienteId = datos.Lead.ClienteId.GetValueOrDefault();
                ViewBag.ModoLead = true;
                ViewBag.Lead = datos.Lead;
            }
            else
            {
                ViewBag.ModoLead = false;
                ViewBag.Clientes = datos.Clientes;
            }

            ViewBag.Equipos = datos.Equipos;
            ViewBag.Instalaciones = datos.Instalaciones;

            // ========== Catálogo lateral: filtrar por empresa ==========
            Guid? empresaId = null;

            if (datos.Lead?.EmpresaId != null)
            {
                empresaId = datos.Lead.EmpresaId.Value;
            }
            else
            {
                var empresaIdString = HttpContext.Session.GetString("EmpresaActivaId");
                if (!string.IsNullOrEmpty(empresaIdString))
                    empresaId = Guid.Parse(empresaIdString);
            }

            if (empresaId.HasValue)
            {
                var empresa = await _empresaRepo.GetByIdAsync(empresaId.Value);
                if (empresa != null && empresa.EsExclusivaTrane)
                {
                    ViewBag.Marcas = new List<TipoMarca> { TipoMarca.Trane };
                }
                else
                {
                    ViewBag.Marcas = Enum.GetValues(typeof(TipoMarca))
                        .Cast<TipoMarca>()
                        .Where(m => m != TipoMarca.Otro)
                        .ToList();
                }
            }
            else
            {
                ViewBag.Marcas = Enum.GetValues(typeof(TipoMarca))
                    .Cast<TipoMarca>()
                    .Where(m => m != TipoMarca.Otro)
                    .ToList();
            }

            ViewBag.MarcaSeleccionada = ViewBag.Marcas.Count == 1 ? ViewBag.Marcas[0] : (TipoMarca?)null;

            return View(viewModel);
        }

        // POST: Cotizacion/Crear
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearCotizacionViewModel formulario)
        {
            if (!ModelState.IsValid)
                return RedirectToAction(nameof(Crear), new { leadId = formulario.LeadId });

            var vendedor = await _userManager.GetUserAsync(User);
            if (vendedor == null)
            {
                TempData["MensajeError"] = "Debe iniciar sesión para crear cotizaciones";
                return RedirectToAction("Login", "Autenticacion");
            }

            Empresa? empresa = null;

            if (formulario.LeadId.HasValue)
            {
                var datosLead = await _cotizacionServicio.ObtenerDatosParaCrearAsync(
                    vendedor.Id, User.IsInRole("Vendedor"), formulario.LeadId);

                if (datosLead.Lead?.EmpresaId != null)
                    empresa = await _empresaRepo.GetByIdAsync(datosLead.Lead.EmpresaId.Value);
            }

            if (empresa == null)
                empresa = await ObtenerEmpresaActual();

            if (empresa == null)
            {
                TempData["MensajeError"] = "No hay empresa activa";
                return RedirectToAction(nameof(Crear), new { leadId = formulario.LeadId });
            }

            var dto = new CrearCotizacionDto
            {
                ClienteId = formulario.ClienteId,
                EmpresaId = empresa.Id,
                VendedorId = vendedor.Id,
                AreaMetrosCuadrados = formulario.AreaMetrosCuadrados,
                CondicionesPago = formulario.CondicionesPago,
                Equipos = DeserializarEquipos(formulario.EquiposJson),
                Instalaciones = DeserializarInstalaciones(formulario.InstalacionesJson),
                LeadId = formulario.LeadId
            };

            var resultado = await _cotizacionServicio.CrearAsync(dto);

            if (!resultado.Exitoso)
            {
                TempData["MensajeError"] = resultado.MensajeError;
                return RedirectToAction(nameof(Crear), new { leadId = formulario.LeadId });
            }

            TempData["MensajeExito"] = $"Cotización {resultado.Cotizacion!.NumeroCotizacion} creada exitosamente";
            return RedirectToAction(nameof(Detalles), new { id = resultado.Cotizacion.Id });
        }
        // GET: Cotizacion/DescargarPdf/5
        [HttpGet]
        public async Task<IActionResult> DescargarPdf(Guid id)
        {
            try
            {
                var pdfBytes = await _cotizacionServicio.GenerarPdfAsync(id);
                var cotizacion = await _cotizacionServicio.ObtenerDetalleAsync(id);
                var nombreArchivo = $"{cotizacion!.NumeroCotizacion}.pdf";
                return File(pdfBytes, "application/pdf", nombreArchivo);
            }
            catch (KeyNotFoundException)
            {
                return NotFound("Cotización no encontrada");
            }
        }

        // POST: Cotizacion/Editar/5
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Editar(Guid id, Guid clienteId, decimal areaMetrosCuadrados, string condicionesPago)
        {
            var dto = new ActualizarCotizacionDto
            {
                Id = id,
                ClienteId = clienteId,
                AreaMetrosCuadrados = areaMetrosCuadrados,
                CondicionesPago = condicionesPago
            };

            var resultado = await _cotizacionServicio.ActualizarAsync(dto);

            if (!resultado.Exitoso)
            {
                TempData["MensajeError"] = resultado.MensajeError;
                return RedirectToAction(nameof(Editar), new { id });
            }

            TempData["MensajeExito"] = "Cotización actualizada";
            return RedirectToAction(nameof(Detalles), new { id });
        }

        // GET: Cotizacion/Eliminar/5
        public async Task<IActionResult> Eliminar(Guid? id)
        {
            if (id == null)
                return NotFound();

            var cotizacion = await _cotizacionServicio.ObtenerDetalleAsync(id.Value);

            if (cotizacion == null)
                return NotFound();

            var viewModel = MapearADetalleViewModel(cotizacion);
            return View(viewModel);
        }

        // POST: Cotizacion/Eliminar/5
        [HttpPost, ActionName("Eliminar")]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EliminarConfirmado(Guid id)
        {
            var resultado = await _cotizacionServicio.EliminarAsync(id);

            if (!resultado.Exitoso)
            {
                TempData["MensajeError"] = resultado.MensajeError;
                return RedirectToAction(nameof(Indice));
            }

            TempData["MensajeExito"] = "Cotización eliminada";
            return RedirectToAction(nameof(Indice));
        }

        // GET: Cotizacion/CalcularCargaTermica
        [HttpGet]
        public async Task<IActionResult> CalcularCargaTermica(decimal area)
        {
            var tr = await _cotizacionServicio.CalcularCargaTermicaAsync(area);
            var btu = tr * 12000;

            return Json(new { tr, btu = Math.Round(btu, 0) });
        }

        // POST: Cotizacion/CambiarEstado
        [HttpPost]
        public async Task<IActionResult> CambiarEstado(Guid cotizacionId, int nuevoEstado)
        {
            var resultado = await _cotizacionServicio.CambiarEstadoAsync(cotizacionId, nuevoEstado);

            if (!resultado.Exitoso)
                return Json(new { success = false, message = resultado.MensajeError });

            return Json(new { success = true, nuevoEstado = ((EstadoCotizacion)nuevoEstado).ToString() });
        }

        // ==================== MÉTODOS AUXILIARES ====================

        private async Task<Empresa?> ObtenerEmpresaActual()
        {
            var empresaIdString = HttpContext.Session.GetString("EmpresaActivaId");
            if (string.IsNullOrEmpty(empresaIdString))
                return await _empresaRepo.ObtenerActivaAsync();

            var empresaId = Guid.Parse(empresaIdString);
            return await _empresaRepo.GetByIdAsync(empresaId);
        }

        private List<ItemCotizacionJson> DeserializarEquipos(string? json)
        {
            if (string.IsNullOrEmpty(json)) return new();
            return System.Text.Json.JsonSerializer.Deserialize<List<ItemCotizacionJson>>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }

        private List<ItemInstalacionJson> DeserializarInstalaciones(string? json)
        {
            if (string.IsNullOrEmpty(json)) return new();
            return System.Text.Json.JsonSerializer.Deserialize<List<ItemInstalacionJson>>(json,
                new System.Text.Json.JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }

        private CotizacionDetalleViewModel MapearADetalleViewModel(CotizacionDetalleDto dto)
        {
            return new CotizacionDetalleViewModel
            {
                Id = dto.Id,
                NumeroCotizacion = dto.NumeroCotizacion,
                ClienteNombre = dto.ClienteNombre,
                EmpresaNombre = dto.EmpresaNombre,
                VendedorNombre = dto.VendedorNombre,
                FechaCreacion = dto.FechaCreacion,
                FechaVencimiento = dto.FechaVencimiento,
                Estado = dto.Estado,
                AreaMetrosCuadrados = dto.AreaMetrosCuadrados,
                CondicionesPago = dto.CondicionesPago,
                Subtotal = dto.Subtotal,
                Iva = dto.Iva,
                Total = dto.Total,
                Moneda = dto.Moneda,
                PuedeSerModificada = dto.PuedeSerModificada,
                ClienteId = dto.ClienteId,
                Equipos = dto.Equipos.Select(e => new ItemCotizacionViewModel
                {
                    EquipoMarca = e.EquipoMarca,
                    EquipoModelo = e.EquipoModelo,
                    Cantidad = e.Cantidad,
                    PrecioUnitario = e.PrecioUnitario,
                    Subtotal = e.Subtotal
                }).ToList(),
                Instalaciones = dto.Instalaciones.Select(i => new ItemInstalacionViewModel
                {
                    Concepto = i.Concepto,
                    Descripcion = i.Descripcion,
                    Cantidad = i.Cantidad,
                    CostoUnitario = i.CostoUnitario,
                    Subtotal = i.Subtotal
                }).ToList()
            };
        }
    }
}