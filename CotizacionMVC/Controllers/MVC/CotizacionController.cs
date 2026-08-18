using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Models.Enums;
using CotizacionMVC.Servicios.Aplicacion.Dtos.Cotizacion;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using CotizacionMVC.ViewModels;
using CotizacionMVC.ViewModels.Cotizacion;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace CotizacionMVC.Controllers.MVC
{
    [Authorize(Roles = "Administrador,Vendedor")]
    public class CotizacionController : Controller
    {
        private readonly ICotizacionServicio _cotizacionServicio;
        private readonly IUserContextService _userContextService;
        private readonly IEmpresaServicio _empresaServicio;

        public CotizacionController(
            ICotizacionServicio cotizacionServicio,
            IUserContextService userContextService,
            IEmpresaServicio empresaServicio)
        {
            _cotizacionServicio = cotizacionServicio;
            _userContextService = userContextService;
            _empresaServicio = empresaServicio;
        }

        public async Task<IActionResult> Indice()
        {
            var cotizaciones = await _cotizacionServicio.ObtenerIndiceAsync();
            var leads = await _cotizacionServicio.ObtenerLeadsAsync();

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
                    Estado = Enum.Parse<EstadoCotizacion>(c.Estado)
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

        [HttpGet]
        public async Task<IActionResult> Crear(Guid? leadId = null)
        {
            var empresaActiva = await _cotizacionServicio.ObtenerEmpresaActivaAsync();
            if (empresaActiva == null)
            {
                TempData["MensajeError"] = "Debe seleccionar una empresa primero";
                return RedirectToAction("Indice", "Empresa");
            }

            var usuarioActual = await _userContextService.GetCurrentUserAsync();
            if (usuarioActual == null)
                return RedirectToAction("Login", "Autenticacion");

            var datos = await _cotizacionServicio.ObtenerDatosParaCrearAsync(leadId);

            var viewModel = new CrearCotizacionViewModel
            {
                LeadId = leadId,
                AreaMetrosCuadrados = 100
            };

            if (datos.Lead != null)
            {
                viewModel.ClienteId = datos.Lead.ClienteId;
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

            if (empresaActiva != null && empresaActiva.EsExclusivaTrane)
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

            ViewBag.MarcaSeleccionada = ViewBag.Marcas.Count == 1 ? ViewBag.Marcas[0] : (TipoMarca?)null;
            ViewBag.InstalacionesCatalogo = await _cotizacionServicio.ObtenerCatalogoInstalacionesAsync();

            ViewBag.EmpresaActiva = empresaActiva;
            ViewBag.EmpresaId = empresaActiva.Id;

            return View(viewModel);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Crear(CrearCotizacionViewModel formulario)
        {
            var empresaActiva = await _cotizacionServicio.ObtenerEmpresaActivaAsync();
            if (empresaActiva == null)
            {
                TempData["MensajeError"] = "Debe seleccionar una empresa primero";
                return RedirectToAction("Indice", "Empresa");
            }

            if (!ModelState.IsValid)
            {
                await RecargarDatosFormulario(formulario);
                return View(formulario);
            }

            if (!formulario.ClienteId.HasValue || formulario.ClienteId.Value == Guid.Empty)
            {
                ModelState.AddModelError("", "Debe seleccionar un cliente");
                await RecargarDatosFormulario(formulario);
                return View(formulario);
            }

            var equipos = DeserializarEquipos(formulario.EquiposJson);
            if (equipos == null || !equipos.Any())
            {
                ModelState.AddModelError("", "Debe agregar al menos un equipo");
                await RecargarDatosFormulario(formulario);
                return View(formulario);
            }

            var instalaciones = DeserializarInstalaciones(formulario.InstalacionesJson);

            var vendedor = await _userContextService.GetCurrentUserAsync();
            if (vendedor == null)
            {
                ModelState.AddModelError("", "Usuario no autenticado");
                await RecargarDatosFormulario(formulario);
                return View(formulario);
            }

            var dto = new CrearCotizacionDto
            {
                ClienteId = formulario.ClienteId.Value,
                EmpresaId = empresaActiva.Id, 
                VendedorId = vendedor.Id,
                AreaMetrosCuadrados = formulario.AreaMetrosCuadrados,
                CondicionesPago = formulario.CondicionesPago ?? string.Empty,
                Equipos = equipos,
                Instalaciones = instalaciones,
                LeadId = formulario.LeadId,
                TipoCambio = 17.43m,
                RecargoCiudadPorcentaje = 0
            };

            var resultado = await _cotizacionServicio.CrearAsync(dto);

            if (!resultado.Exitoso)
            {
                ModelState.AddModelError("", resultado.MensajeError ?? "Error al crear la cotización");
                await RecargarDatosFormulario(formulario);
                return View(formulario);
            }

            TempData["MensajeExito"] = $"Cotización {resultado.Cotizacion!.NumeroCotizacion} creada exitosamente";
            return RedirectToAction(nameof(Detalles), new { id = resultado.Cotizacion.Id });
        }

        private async Task RecargarDatosFormulario(CrearCotizacionViewModel formulario)
        {
            var empresaActiva = await _cotizacionServicio.ObtenerEmpresaActivaAsync();
            var datos = await _cotizacionServicio.ObtenerDatosParaCrearAsync(formulario.LeadId);
            var instalacionesCatalogo = await _cotizacionServicio.ObtenerCatalogoInstalacionesAsync();

            if (datos.Lead != null)
            {
                ViewBag.ModoLead = true;
                ViewBag.Lead = datos.Lead;
                if (!formulario.ClienteId.HasValue)
                {
                    formulario.ClienteId = datos.Lead.ClienteId;
                }
            }
            else
            {
                ViewBag.ModoLead = false;
                ViewBag.Clientes = datos.Clientes;
            }

            ViewBag.Equipos = datos.Equipos;
            ViewBag.Instalaciones = datos.Instalaciones;
            ViewBag.InstalacionesCatalogo = instalacionesCatalogo;
            ViewBag.EmpresaActiva = empresaActiva;
            ViewBag.EmpresaId = empresaActiva?.Id ?? Guid.Empty;

            if (empresaActiva != null && empresaActiva.EsExclusivaTrane)
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

            ViewBag.MarcaSeleccionada = ViewBag.Marcas.Count == 1 ? ViewBag.Marcas[0] : (TipoMarca?)null;

            ViewBag.EquiposJson = formulario.EquiposJson;
            ViewBag.InstalacionesJson = formulario.InstalacionesJson;
        }

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
            catch (Exception ex)
            {
                TempData["MensajeError"] = $"Error al generar PDF: {ex.Message}";
                return RedirectToAction(nameof(Detalles), new { id });
            }
        }

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

        [HttpGet]
        public async Task<IActionResult> CalcularCargaTermica(decimal area)
        {
            var tr = await _cotizacionServicio.CalcularCargaTermicaAsync(area);
            var btu = tr * 12000;

            return Json(new { tr, btu = Math.Round(btu, 0) });
        }

        [HttpPost]
        public async Task<IActionResult> CambiarEstado(Guid cotizacionId, int nuevoEstado)
        {
            var resultado = await _cotizacionServicio.CambiarEstadoAsync(cotizacionId, nuevoEstado);

            if (!resultado.Exitoso)
                return Json(new { success = false, message = resultado.MensajeError });

            return Json(new { success = true, nuevoEstado = ((EstadoCotizacion)nuevoEstado).ToString() });
        }

        private List<ItemCotizacionJson> DeserializarEquipos(string? json)
        {
            if (string.IsNullOrEmpty(json)) return new();
            return JsonSerializer.Deserialize<List<ItemCotizacionJson>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
        }

        private List<ItemInstalacionJson> DeserializarInstalaciones(string? json)
        {
            if (string.IsNullOrEmpty(json)) return new();
            return JsonSerializer.Deserialize<List<ItemInstalacionJson>>(json,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ?? new();
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