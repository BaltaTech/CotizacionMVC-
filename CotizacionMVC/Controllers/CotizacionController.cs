using CotizacionMVC.Data;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Models.Enums;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

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
            var listaEquipos = string.IsNullOrEmpty(equipos)
                ? new List<ItemCotizacionJson>()
                : System.Text.Json.JsonSerializer.Deserialize<List<ItemCotizacionJson>>(equipos);

            var listaInstalaciones = string.IsNullOrEmpty(instalaciones)
                ? new List<ItemInstalacionJson>()
                : System.Text.Json.JsonSerializer.Deserialize<List<ItemInstalacionJson>>(instalaciones);

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

            var nombreVendedor = User.Identity?.Name ?? "Vendedor";
            var vendedor = await _contextoBaseDatos.Usuarios
                .FirstOrDefaultAsync(u => u.NombreCompleto == nombreVendedor);

            if (vendedor == null)
            {
                var correoTemporal = $"{nombreVendedor.Replace(" ", ".").ToLower()}@sistema.local";
                vendedor = new Usuario(nombreVendedor, correoTemporal, RolUsuario.Vendedor);
                _contextoBaseDatos.Usuarios.Add(vendedor);
                await _contextoBaseDatos.SaveChangesAsync();
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

            await _contextoBaseDatos.SaveChangesAsync();

            TempData["MensajeExito"] = $"Cotización {numeroCotizacion} creada exitosamente";
            return RedirectToAction(nameof(Detalles), new { id = cotizacion.Id });
        }

        // GET: Cotizacion/Editar/5
        public async Task<IActionResult> Editar(Guid? id)
        {
            if (id == null)
                return NotFound();

            var cotizacion = await _contextoBaseDatos.Cotizaciones
                .Include(c => c.ItemsEquipos)
                .Include(c => c.ItemsInstalacion)
                .FirstOrDefaultAsync(c => c.Id == id);

            if (cotizacion == null)
                return NotFound();

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

        // GET: Cotizacion/DescargarPdf/5
        [HttpGet]
        public async Task<IActionResult> DescargarPdf(Guid id)
        {
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
                return NotFound("Cotización no encontrada");

            // Si ya tiene PDF, devolverlo directo
            if (!string.IsNullOrEmpty(cotizacion.RutaPdf))
            {
                var rutaExistente = Path.Combine(Directory.GetCurrentDirectory(), cotizacion.RutaPdf);
                if (System.IO.File.Exists(rutaExistente))
                {
                    var bytesExistentes = await System.IO.File.ReadAllBytesAsync(rutaExistente);
                    return File(bytesExistentes, "application/pdf", $"{cotizacion.NumeroCotizacion}.pdf");
                }
            }

            // Generar PDF
            var pdfBytes = await GenerarPdfCotizacion(cotizacion);

            // Guardar archivo
            var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdf", "cotizaciones");
            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            var nombreArchivo = $"{cotizacion.NumeroCotizacion}.pdf";
            var rutaCompleta = Path.Combine(carpeta, nombreArchivo);
            await System.IO.File.WriteAllBytesAsync(rutaCompleta, pdfBytes);

            // Guardar ruta en BD
            var rutaRelativa = $"wwwroot/pdf/cotizaciones/{nombreArchivo}";
            cotizacion.GuardarRutaPdf(rutaRelativa);
            await _contextoBaseDatos.SaveChangesAsync();

            return File(pdfBytes, "application/pdf", nombreArchivo);
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

        private async Task<byte[]> GenerarPdfCotizacion(Cotizacion cotizacion)
        {
            var empresa = cotizacion.Empresa;

            byte[]? logoBytes = null;
            if (!string.IsNullOrEmpty(empresa.LogoUrl))
            {
                var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", empresa.LogoUrl.TrimStart('/'));
                if (System.IO.File.Exists(logoPath))
                    logoBytes = await System.IO.File.ReadAllBytesAsync(logoPath);
            }

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(40);

                    var colorPrimario = ParseColor(empresa.ColorPrimario ?? "#1A365D");
                    var colorSecundario = ParseColor(empresa.ColorSecundario ?? "#2D3748");
                    var colorGris = ParseColor("#718096");

                    // ─── ENCABEZADO ───
                    page.Header().Element(header =>
                    {
                        header.Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                if (logoBytes != null)
                                    row.ConstantItem(80).Image(logoBytes);

                                row.RelativeItem().Column(columna =>
                                {
                                    columna.Item().Text(empresa.NombreComercial)
                                        .FontSize(20).Bold().FontColor(colorPrimario);

                                    if (!string.IsNullOrEmpty(empresa.Eslogan))
                                        columna.Item().Text(empresa.Eslogan)
                                            .FontSize(9).FontColor(colorGris).Italic();
                                });

                                row.ConstantItem(130).Column(numero =>
                                {
                                    numero.Item().AlignRight().Text("COTIZACIÓN")
                                        .FontSize(10).Bold().FontColor(colorGris);
                                    numero.Item().AlignRight().Text(cotizacion.NumeroCotizacion)
                                        .FontSize(18).Bold().FontColor(colorPrimario);
                                    numero.Item().PaddingTop(3).AlignRight().Text($"Fecha: {cotizacion.FechaCreacion:dd/MM/yyyy}")
                                        .FontSize(8).FontColor(colorGris);
                                    numero.Item().AlignRight().Text($"Vence: {cotizacion.FechaVencimiento:dd/MM/yyyy}")
                                        .FontSize(8).FontColor(colorGris);
                                });
                            });

                            col.Item().PaddingTop(10).LineHorizontal(1).LineColor(colorPrimario);
                        });
                    });

                    // ─── CONTENIDO ───
                    page.Content().Column(content =>
                    {
                        content.Item().PaddingTop(20).Row(datos =>
                        {
                            datos.RelativeItem().Column(cliente =>
                            {
                                cliente.Item().Text("CLIENTE").FontSize(9).Bold().FontColor(colorGris);
                                cliente.Item().PaddingTop(3).LineHorizontal(1).LineColor(colorPrimario);
                                cliente.Item().PaddingTop(6).Text(cotizacion.Cliente.Nombre)
                                    .FontSize(13).Bold().FontColor(colorSecundario);
                                cliente.Item().Text(cotizacion.Cliente.ObtenerContactoPrincipal())
                                    .FontSize(9).FontColor(colorGris);
                            });

                            datos.ConstantItem(180).Column(vendedor =>
                            {
                                vendedor.Item().Text("DATOS COMERCIALES").FontSize(9).Bold().FontColor(colorGris);
                                vendedor.Item().PaddingTop(3).LineHorizontal(1).LineColor(colorPrimario);
                                vendedor.Item().PaddingTop(6).Text($"Vendedor: {cotizacion.Vendedor.NombreCompleto}")
                                    .FontSize(9).FontColor(colorSecundario);
                                vendedor.Item().Text($"Área: {cotizacion.AreaMetrosCuadrados:N0} m²")
                                    .FontSize(9).FontColor(colorSecundario);
                                vendedor.Item().Text($"Condiciones: {cotizacion.CondicionesPago}")
                                    .FontSize(9).FontColor(colorSecundario);
                            });
                        });

                        // EQUIPOS
                        content.Item().PaddingTop(20).Text("EQUIPOS COTIZADOS")
                            .FontSize(10).Bold().FontColor(colorPrimario);
                        content.Item().PaddingTop(10);

                        content.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50);
                                columns.RelativeColumn();
                                columns.ConstantColumn(100);
                                columns.ConstantColumn(100);
                            });

                            tabla.Header(header =>
                            {
                                header.Cell().BorderBottom(1).BorderColor(colorPrimario).PaddingBottom(5)
                                    .Text("Cant").FontSize(9).Bold().FontColor(colorPrimario);
                                header.Cell().BorderBottom(1).BorderColor(colorPrimario).PaddingBottom(5)
                                    .Text("Descripción").FontSize(9).Bold().FontColor(colorPrimario);
                                header.Cell().BorderBottom(1).BorderColor(colorPrimario).PaddingBottom(5).AlignRight()
                                    .Text("P. Unitario").FontSize(9).Bold().FontColor(colorPrimario);
                                header.Cell().BorderBottom(1).BorderColor(colorPrimario).PaddingBottom(5).AlignRight()
                                    .Text("Subtotal").FontSize(9).Bold().FontColor(colorPrimario);
                            });

                            foreach (var item in cotizacion.ItemsEquipos)
                            {
                                tabla.Cell().PaddingVertical(5)
                                    .Text(item.Cantidad.ToString()).FontSize(10);
                                tabla.Cell().PaddingVertical(5)
                                    .Text($"{item.Equipo.Marca} {item.Equipo.Modelo}").FontSize(10);
                                tabla.Cell().PaddingVertical(5).AlignRight()
                                    .Text($"{item.PrecioUnitario.Monto:N2}").FontSize(10);
                                tabla.Cell().PaddingVertical(5).AlignRight()
                                    .Text($"{item.Subtotal.Monto:N2}").FontSize(10).Bold();
                            }
                        });

                        // INSTALACIONES
                        if (cotizacion.ItemsInstalacion.Any())
                        {
                            content.Item().PaddingTop(20).Text("SERVICIOS E INSTALACIONES")
                                .FontSize(10).Bold().FontColor(colorPrimario);
                            content.Item().PaddingTop(10);

                            content.Item().Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.RelativeColumn();
                                    columns.ConstantColumn(100);
                                    columns.ConstantColumn(100);
                                });

                                tabla.Header(header =>
                                {
                                    header.Cell().BorderBottom(1).BorderColor(colorPrimario).PaddingBottom(5)
                                        .Text("Concepto").FontSize(9).Bold().FontColor(colorPrimario);
                                    header.Cell().BorderBottom(1).BorderColor(colorPrimario).PaddingBottom(5).AlignRight()
                                        .Text("Costo Unit.").FontSize(9).Bold().FontColor(colorPrimario);
                                    header.Cell().BorderBottom(1).BorderColor(colorPrimario).PaddingBottom(5).AlignRight()
                                        .Text("Subtotal").FontSize(9).Bold().FontColor(colorPrimario);
                                });

                                foreach (var inst in cotizacion.ItemsInstalacion)
                                {
                                    tabla.Cell().PaddingVertical(5)
                                        .Text(inst.Concepto).FontSize(10);
                                    tabla.Cell().PaddingVertical(5).AlignRight()
                                        .Text($"{inst.CostoUnitario:N2}").FontSize(10);
                                    tabla.Cell().PaddingVertical(5).AlignRight()
                                        .Text($"{inst.Subtotal.Monto:N2}").FontSize(10).Bold();
                                }
                            });
                        }

                        // TOTALES
                        content.Item().PaddingTop(20).AlignRight().Column(totales =>
                        {
                            totales.Item().Text($"Subtotal: {cotizacion.Subtotal.Monto:N2}").FontSize(10).FontColor(colorSecundario);
                            totales.Item().Text($"IVA (16%): {cotizacion.Iva.Monto:N2}").FontSize(10).FontColor(colorSecundario);
                            totales.Item().PaddingTop(5).Text($"TOTAL: {cotizacion.Total.Monto:N2}")
                                .FontSize(14).Bold().FontColor(colorPrimario);
                        });

                        // CONDICIONES
                        content.Item().PaddingTop(20).Text("CONDICIONES").FontSize(9).Bold().FontColor(colorGris);
                        content.Item().Text(cotizacion.CondicionesPago).FontSize(9).FontColor(colorSecundario);
                        content.Item().Text($"Vendedor: {cotizacion.Vendedor.NombreCompleto}").FontSize(9).FontColor(colorSecundario);

                        // AVISO AUTORIZACIÓN
                        if (cotizacion.RequiereAutorizacion)
                        {
                            content.Item().PaddingTop(15).Text("⚠ Esta cotización requiere autorización por ser mayor a $500,000 MXN")
                                .FontSize(9).FontColor(ParseColor("#975A16"));
                        }
                    });

                    // ─── PIE DE PÁGINA ───
                    page.Footer().Element(footer =>
                    {
                        footer.AlignCenter().Text(text =>
                        {
                            text.Span($"{empresa.NombreComercial} - Página ").FontSize(8).FontColor(colorGris);
                            text.CurrentPageNumber();
                            text.Span(" de ").FontSize(8).FontColor(colorGris);
                            text.TotalPages();
                        });
                    });
                });
            }).GeneratePdf();
        }

        private static QuestPDF.Infrastructure.Color ParseColor(string hex)
        {
            if (string.IsNullOrEmpty(hex))
                hex = "#3B82F6";

            if (hex.StartsWith("#"))
                hex = hex.Substring(1);

            if (hex.Length == 6)
            {
                var r = Convert.ToByte(hex.Substring(0, 2), 16);
                var g = Convert.ToByte(hex.Substring(2, 2), 16);
                var b = Convert.ToByte(hex.Substring(4, 2), 16);
                return QuestPDF.Infrastructure.Color.FromRGB(r, g, b);
            }

            return QuestPDF.Infrastructure.Color.FromRGB(59, 130, 246);
        }
    }

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