using CotizacionMVC.Models.Entidades;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CotizacionMVC.Servicios
{
    public class PdfCotizacion : IDocumento
    {
        private string _primary;
        private string _primaryLight;
        private string _accent;
        private string _textPrimary;
        private string _textSecondary;
        private string _textMuted;
        private string _surface;
        private string _surfaceHover;
        private string _border;

        private const string FontFamily = "Arial, Helvetica, sans-serif";
        private const float FontSizeSmall = 8;
        private const float FontSizeBody = 9.5f;
        private const float FontSizeTitle = 11;
        private const float FontSizeHeading = 13;
        private const float FontSizeDisplay = 18;

        public string TipoContenido => "application/pdf";
        public string ExtensionArchivo => ".pdf";

        public byte[] Generar(Cotizacion cotizacion)
        {
            var empresa = cotizacion.Empresa;

            _primary = HexToColor(empresa.ColorPrimario ?? "#0F172A");
            _primaryLight = LightenColor(_primary, 0.4f);
            _accent = _primary;
            _textPrimary = HexToColor("#1E293B"); // Gris oscuro premium
            _textSecondary = HexToColor("#475569");
            _textMuted = HexToColor("#64748B");
            _surface = HexToColor("#F8FAFC");
            _surfaceHover = HexToColor("#F1F5F9");
            _border = HexToColor("#E2E8F0");

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(30); // Margen optimizado para aprovechar la hoja
                    page.DefaultTextStyle(x => x.FontFamily(FontFamily).FontColor(_textPrimary));

                    page.Header().Element(h => ConstruirHeader(h, empresa, cotizacion));

                    page.Content().Column(content =>
                    {
                        ConstruirInformacionCliente(content, cotizacion);
                        ConstruirTablaEquipos(content, cotizacion);
                        ConstruirTablaInstalaciones(content, cotizacion);
                        ConstruirResumenYTerminos(content, cotizacion);
                        ConstruirFirmas(content, cotizacion);
                    });

                    page.Footer().Element(f => ConstruirFooter(f, empresa));
                });
            }).GeneratePdf();
        }

        private void ConstruirHeader(IContainer container, Empresa empresa, Cotizacion cotizacion)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    // Lado Izquierdo: Logo y Nombre de Empresa
                    row.RelativeItem().Row(logoRow =>
                    {
                        var logoBytes = CargarLogo(empresa.LogoUrl);
                        if (logoBytes != null)
                        {
                            logoRow.ConstantItem(65).MaxHeight(50).Image(logoBytes).FitArea();
                        }

                        logoRow.RelativeItem().PaddingLeft(10).Column(emp =>
                        {
                            emp.Item().Text(empresa.NombreComercial)
                                .FontSize(FontSizeDisplay).Bold().FontColor(_primary);

                            if (!string.IsNullOrEmpty(empresa.Eslogan))
                            {
                                emp.Item().Text(empresa.Eslogan)
                                    .FontSize(FontSizeSmall).FontColor(_textMuted).Italic();
                            }
                        });
                    });

                    // Lado Derecho: Datos de la Cotización (Sin saltos de letra extraños)
                    row.RelativeItem().AlignRight().Column(cotiz =>
                    {
                        cotiz.Item().Text("COTIZACIÓN")
                            .FontSize(FontSizeTitle).Bold().FontColor(_primary);

                        cotiz.Item().Text($"# {cotizacion.NumeroCotizacion}")
                            .FontSize(FontSizeHeading).Bold().FontColor(_textSecondary);

                        cotiz.Item().PaddingTop(2).Text(text =>
                        {
                            text.Span("Emisión: ").FontSize(FontSizeSmall).FontColor(_textMuted);
                            text.Span(cotizacion.FechaCreacion.ToString("dd/MM/yyyy"))
                                .FontSize(FontSizeSmall).FontColor(_textPrimary).Bold();
                        });

                        cotiz.Item().Text(text =>
                        {
                            text.Span("Vigencia: ").FontSize(FontSizeSmall).FontColor(_textMuted);
                            text.Span(cotizacion.FechaVencimiento.ToString("dd/MM/yyyy"))
                                .FontSize(FontSizeSmall).FontColor(_accent).Bold();
                        });
                    });
                });

                col.Item().PaddingTop(10).LineHorizontal(2).LineColor(_primary);
            });
        }

        private void ConstruirInformacionCliente(ColumnDescriptor content, Cotizacion cotizacion)
        {
            // Bloque unificado con borde sutil para mejor estructura visual
            content.Item().PaddingTop(12).Border(1).BorderColor(_border).Background(_surface).Padding(12).Row(row =>
            {
                // Columna Cliente
                row.RelativeItem().Column(col =>
                {
                    col.Item().Text("CLIENTE")
                        .FontSize(FontSizeSmall).Bold().FontColor(_primary);

                    col.Item().PaddingTop(2)
                        .Text(cotizacion.Cliente.Nombre)
                        .FontSize(FontSizeTitle).Bold().FontColor(_textPrimary);

                    col.Item().Text(cotizacion.Cliente.ObtenerContactoPrincipal())
                        .FontSize(FontSizeBody).FontColor(_textSecondary);
                });

                // Divisor Vertical sutil
                row.ConstantItem(1).Height(45).Background(_border);

                // Columna Datos Comerciales
                row.RelativeItem().PaddingLeft(14).Column(col =>
                {
                    col.Item().Text("DATOS COMERCIALES")
                        .FontSize(FontSizeSmall).Bold().FontColor(_primary);

                    col.Item().PaddingTop(2).Text(text =>
                    {
                        text.Span("Ejecutivo: ").FontSize(FontSizeBody).Bold().FontColor(_textPrimary);
                        text.Span(cotizacion.Vendedor.NombreCompleto).FontSize(FontSizeBody).FontColor(_textSecondary);
                    });

                    col.Item().Text(text =>
                    {
                        text.Span("Área: ").FontSize(FontSizeBody).Bold().FontColor(_textPrimary);
                        text.Span($"{cotizacion.AreaMetrosCuadrados:N0} m²").FontSize(FontSizeBody).FontColor(_textSecondary);
                    });

                    col.Item().Text(text =>
                    {
                        text.Span("Condiciones: ").FontSize(FontSizeBody).Bold().FontColor(_textPrimary);
                        text.Span(cotizacion.CondicionesPago).FontSize(FontSizeBody).FontColor(_textSecondary);
                    });
                });
            });
        }

        private void ConstruirTablaEquipos(ColumnDescriptor content, Cotizacion cotizacion)
        {
            content.Item().PaddingTop(14)
                .Text("EQUIPOS Y MATERIALES")
                .FontSize(FontSizeTitle).Bold().FontColor(_primary);

            content.Item().PaddingTop(4).Table(tabla =>
            {
                tabla.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);
                    columns.RelativeColumn(4);
                    columns.ConstantColumn(40);
                    columns.ConstantColumn(85);
                    columns.ConstantColumn(90);
                });

                tabla.Header(header =>
                {
                    header.Cell().Background(_primary).PaddingVertical(5).PaddingHorizontal(6).Text("#").FontSize(FontSizeSmall).Bold().FontColor("#FFFFFF");
                    header.Cell().Background(_primary).PaddingVertical(5).PaddingHorizontal(6).Text("DESCRIPCIÓN").FontSize(FontSizeSmall).Bold().FontColor("#FFFFFF");
                    header.Cell().Background(_primary).PaddingVertical(5).PaddingHorizontal(6).AlignCenter().Text("CANT").FontSize(FontSizeSmall).Bold().FontColor("#FFFFFF");
                    header.Cell().Background(_primary).PaddingVertical(5).PaddingHorizontal(6).AlignRight().Text("PRECIO UNIT.").FontSize(FontSizeSmall).Bold().FontColor("#FFFFFF");
                    header.Cell().Background(_primary).PaddingVertical(5).PaddingHorizontal(6).AlignRight().Text("SUBTOTAL").FontSize(FontSizeSmall).Bold().FontColor("#FFFFFF");
                });

                int index = 1;
                foreach (var item in cotizacion.ItemsEquipos)
                {
                    string bgColor = (index % 2 == 0) ? _surfaceHover : "#FFFFFF";

                    tabla.Cell().Background(bgColor).Padding(5).Text(index.ToString()).FontSize(FontSizeBody).FontColor(_textMuted);
                    tabla.Cell().Background(bgColor).Padding(5).Text($"{item.Equipo.Marca} {item.Equipo.Modelo}").FontSize(FontSizeBody).FontColor(_textPrimary);
                    tabla.Cell().Background(bgColor).Padding(5).AlignCenter().Text(item.Cantidad.ToString()).FontSize(FontSizeBody).FontColor(_textPrimary);
                    tabla.Cell().Background(bgColor).Padding(5).AlignRight().Text($"${item.PrecioUnitario.Monto:N2}").FontSize(FontSizeBody).FontColor(_textSecondary);
                    tabla.Cell().Background(bgColor).Padding(5).AlignRight().Text($"${item.Subtotal.Monto:N2}").FontSize(FontSizeBody).Bold().FontColor(_textPrimary);

                    index++;
                }
            });
        }

        private void ConstruirTablaInstalaciones(ColumnDescriptor content, Cotizacion cotizacion)
        {
            if (!cotizacion.ItemsInstalacion.Any()) return;

            content.Item().PaddingTop(12)
                .Text("SERVICIOS E INSTALACIONES")
                .FontSize(FontSizeTitle).Bold().FontColor(_primary);

            content.Item().PaddingTop(4).Table(tabla =>
            {
                tabla.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(25);
                    columns.RelativeColumn(2);
                    columns.RelativeColumn(3);
                    columns.ConstantColumn(40);
                    columns.ConstantColumn(85);
                    columns.ConstantColumn(90);
                });

                tabla.Header(header =>
                {
                    header.Cell().Background(_primaryLight).PaddingVertical(5).PaddingHorizontal(6).Text("#").FontSize(FontSizeSmall).Bold().FontColor(_primary);
                    header.Cell().Background(_primaryLight).PaddingVertical(5).PaddingHorizontal(6).Text("CONCEPTO").FontSize(FontSizeSmall).Bold().FontColor(_primary);
                    header.Cell().Background(_primaryLight).PaddingVertical(5).PaddingHorizontal(6).Text("DESCRIPCIÓN").FontSize(FontSizeSmall).Bold().FontColor(_primary);
                    header.Cell().Background(_primaryLight).PaddingVertical(5).PaddingHorizontal(6).AlignCenter().Text("CANT").FontSize(FontSizeSmall).Bold().FontColor(_primary);
                    header.Cell().Background(_primaryLight).PaddingVertical(5).PaddingHorizontal(6).AlignRight().Text("COSTO UNIT.").FontSize(FontSizeSmall).Bold().FontColor(_primary);
                    header.Cell().Background(_primaryLight).PaddingVertical(5).PaddingHorizontal(6).AlignRight().Text("SUBTOTAL").FontSize(FontSizeSmall).Bold().FontColor(_primary);
                });

                int index = 1;
                foreach (var inst in cotizacion.ItemsInstalacion)
                {
                    string bgColor = (index % 2 == 0) ? _surfaceHover : "#FFFFFF";

                    var descripcion = !string.IsNullOrWhiteSpace(inst.Descripcion)
                        ? inst.Descripcion
                        : (inst.Instalacion?.Descripcion ?? "-");

                    tabla.Cell().Background(bgColor).Padding(5).Text(index.ToString()).FontSize(FontSizeBody).FontColor(_textMuted);
                    tabla.Cell().Background(bgColor).Padding(5).Text(inst.Concepto).FontSize(FontSizeBody).FontColor(_textPrimary);
                    tabla.Cell().Background(bgColor).Padding(5).Text(descripcion).FontSize(FontSizeSmall).FontColor(_textSecondary);
                    tabla.Cell().Background(bgColor).Padding(5).AlignCenter().Text(inst.Cantidad.ToString()).FontSize(FontSizeBody).FontColor(_textPrimary);
                    tabla.Cell().Background(bgColor).Padding(5).AlignRight().Text($"${inst.CostoUnitario.Monto:N2}").FontSize(FontSizeBody).FontColor(_textSecondary);
                    tabla.Cell().Background(bgColor).Padding(5).AlignRight().Text($"${inst.Subtotal.Monto:N2}").FontSize(FontSizeBody).Bold().FontColor(_textPrimary);

                    index++;
                }
            });
        }

        private void ConstruirResumenYTerminos(ColumnDescriptor content, Cotizacion cotizacion)
        {
            content.Item().PaddingTop(14).Row(row =>
            {
                // Términos y Condiciones
                row.RelativeItem().PaddingRight(15).Column(terms =>
                {
                    terms.Item().Text("TÉRMINOS Y CONDICIONES")
                        .FontSize(FontSizeTitle).Bold().FontColor(_primary);

                    terms.Item().PaddingTop(3).Text(text =>
                    {
                        text.Span("• ").FontSize(FontSizeSmall).FontColor(_accent);
                        text.Span("Garantía: ").FontSize(FontSizeSmall).Bold().FontColor(_textPrimary);
                        text.Span("1 año en partes y 5 años en compresor").FontSize(FontSizeSmall).FontColor(_textSecondary);
                    });

                    terms.Item().Text(text =>
                    {
                        text.Span("• ").FontSize(FontSizeSmall).FontColor(_accent);
                        text.Span("Vigencia: ").FontSize(FontSizeSmall).Bold().FontColor(_textPrimary);
                        text.Span("30 días naturales a partir de la emisión").FontSize(FontSizeSmall).FontColor(_textSecondary);
                    });

                    terms.Item().Text(text =>
                    {
                        text.Span("• ").FontSize(FontSizeSmall).FontColor(_accent);
                        text.Span("Disponibilidad: ").FontSize(FontSizeSmall).Bold().FontColor(_textPrimary);
                        text.Span("Sujeto a inventario al momento del anticipo").FontSize(FontSizeSmall).FontColor(_textSecondary);
                    });

                    terms.Item().Text(text =>
                    {
                        text.Span("• ").FontSize(FontSizeSmall).FontColor(_accent);
                        text.Span("IVA: ").FontSize(FontSizeSmall).Bold().FontColor(_textPrimary);
                        text.Span("16% incluido en el total").FontSize(FontSizeSmall).FontColor(_textSecondary);
                    });

                    if (cotizacion.RequiereAutorizacion)
                    {
                        terms.Item().PaddingTop(6)
                            .Background(HexToColor("#FEF2F2"))
                            .Padding(6).Border(1).BorderColor(HexToColor("#FCA5A5"))
                            .Text("REQUIERE AUTORIZACIÓN DE DIRECCIÓN")
                            .FontSize(FontSizeSmall).Bold().FontColor(HexToColor("#DC2626")).AlignCenter();
                    }
                });

                // Caja de Totales Optimizada y con Borde Elegante
                row.ConstantItem(240).Border(1).BorderColor(_border).Background(_surface).Padding(12).Column(totales =>
                {
                    totales.Item().Text("RESUMEN DE COTIZACIÓN")
                        .FontSize(FontSizeSmall).Bold().FontColor(_textMuted).AlignCenter();

                    totales.Item().PaddingTop(4).Row(r =>
                    {
                        r.RelativeItem().Text("Subtotal:").FontSize(FontSizeBody).FontColor(_textSecondary);
                        r.RelativeItem().AlignRight().Text($"$ {cotizacion.Subtotal.Monto:N2}").FontSize(FontSizeBody).FontColor(_textPrimary);
                    });

                    totales.Item().Row(r =>
                    {
                        r.RelativeItem().Text("IVA (16%):").FontSize(FontSizeBody).FontColor(_textSecondary);
                        r.RelativeItem().AlignRight().Text($"$ {cotizacion.Iva.Monto:N2}").FontSize(FontSizeBody).FontColor(_textPrimary);
                    });

                    totales.Item().PaddingVertical(4).LineHorizontal(1).LineColor(_border);

                    totales.Item().Row(r =>
                    {
                        r.RelativeItem().Text("TOTAL:").FontSize(FontSizeHeading).Bold().FontColor(_primary);
                        r.RelativeItem().AlignRight().Text($"$ {cotizacion.Total.Monto:N2}").FontSize(FontSizeHeading).Bold().FontColor(_accent);
                    });

                    totales.Item().PaddingTop(4)
                        .Text("Válido por 30 días naturales")
                        .FontSize(FontSizeSmall - 1).FontColor(_textMuted).Italic().AlignCenter();
                });
            });
        }

        private void ConstruirFirmas(ColumnDescriptor content, Cotizacion cotizacion)
        {
            content.Item().PaddingTop(30).Row(row =>
            {
                row.RelativeItem().AlignCenter().Column(col =>
                {
                    col.Item().Width(180).LineHorizontal(1).LineColor(_border);
                    col.Item().PaddingTop(3)
                        .Text("Cliente (Nombre y Firma)").FontSize(FontSizeBody).FontColor(_textMuted);
                });

                row.RelativeItem().AlignCenter().Column(col =>
                {
                    col.Item().Width(180).LineHorizontal(1).LineColor(_border);
                    col.Item().PaddingTop(3)
                        .Text(cotizacion.Vendedor.NombreCompleto).FontSize(FontSizeBody).Bold().FontColor(_textPrimary);
                    col.Item()
                        .Text("Representante Comercial").FontSize(FontSizeSmall).FontColor(_textMuted);
                });
            });
        }

        private void ConstruirFooter(IContainer container, Empresa empresa)
        {
            container.Column(col =>
            {
                col.Item().LineHorizontal(1).LineColor(_border);

                col.Item().PaddingTop(4).Row(row =>
                {
                    var partesContacto = new List<string>();
                    if (!string.IsNullOrWhiteSpace(empresa.TelefonoContacto))
                        partesContacto.Add(empresa.TelefonoContacto);
                    if (!string.IsNullOrWhiteSpace(empresa.CorreoContacto))
                        partesContacto.Add(empresa.CorreoContacto);

                    var textoContacto = partesContacto.Any()
                        ? string.Join("  |  ", partesContacto)
                        : empresa.NombreComercial;

                    row.RelativeItem()
                        .Text(textoContacto)
                        .FontSize(FontSizeSmall)
                        .FontColor(_textMuted);

                    row.ConstantItem(100).AlignRight().Text(text =>
                    {
                        text.Span("Página ").FontSize(FontSizeSmall).FontColor(_textMuted);
                        text.CurrentPageNumber().FontSize(FontSizeSmall).Bold().FontColor(_primary);
                        text.Span(" de ").FontSize(FontSizeSmall).FontColor(_textMuted);
                        text.TotalPages().FontSize(FontSizeSmall).Bold().FontColor(_primary);
                    });
                });
            });
        }

        // ==================== UTILITARIOS ====================

        private static byte[]? CargarLogo(string? logoUrl)
        {
            if (string.IsNullOrEmpty(logoUrl)) return null;
            try
            {
                var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", logoUrl.TrimStart('/'));
                if (File.Exists(logoPath)) return File.ReadAllBytes(logoPath);
            }
            catch { }
            return null;
        }

        private static string HexToColor(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return Colors.Grey.Darken3;
            if (!hex.StartsWith("#")) hex = "#" + hex;
            return hex;
        }

        private static string LightenColor(string hex, float amount)
        {
            if (string.IsNullOrEmpty(hex)) return Colors.Grey.Darken3;
            hex = hex.TrimStart('#');
            if (hex.Length != 6) return Colors.Grey.Darken3;

            try
            {
                var r = Convert.ToByte(hex.Substring(0, 2), 16);
                var g = Convert.ToByte(hex.Substring(2, 2), 16);
                var b = Convert.ToByte(hex.Substring(4, 2), 16);

                r = (byte)Math.Min(255, r + (255 - r) * amount);
                g = (byte)Math.Min(255, g + (255 - g) * amount);
                b = (byte)Math.Min(255, b + (255 - b) * amount);

                return $"#{r:X2}{g:X2}{b:X2}";
            }
            catch
            {
                return Colors.Grey.Darken3;
            }
        }
    }
}