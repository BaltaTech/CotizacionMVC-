using CotizacionMVC.Models.Entidades;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CotizacionMVC.Servicios
{
    /// <summary>
    /// Servicio profesional para generación de documentos PDF con diseño corporativo
    /// </summary>
    public class PdfCotizacion : IDocumento
    {
        // ============================================================
        // CONSTANTES DE DISEÑO (UI/UX)
        // ============================================================
        private static class DesignTokens
        {
            // Paleta de colores corporativos (Slate + Primary)
            public static readonly Color Primary = Color.FromRGB(15, 23, 42);      // Slate 900
            public static readonly Color PrimaryLight = Color.FromRGB(30, 41, 59); // Slate 800
            public static readonly Color Accent = Color.FromRGB(220, 38, 38);      // Red 600

            public static readonly Color TextPrimary = Color.FromRGB(15, 23, 42);   // Slate 900
            public static readonly Color TextSecondary = Color.FromRGB(71, 85, 105); // Slate 600
            public static readonly Color TextMuted = Color.FromRGB(100, 116, 139);   // Slate 500

            public static readonly Color Surface = Color.FromRGB(248, 250, 252);     // Slate 50
            public static readonly Color SurfaceHover = Color.FromRGB(241, 245, 249); // Slate 100
            public static readonly Color Border = Color.FromRGB(226, 232, 240);      // Slate 200

            // Tipografía
            public const string FontFamily = "Inter, Arial, sans-serif";
            public const float FontSizeSmall = 8;
            public const float FontSizeBody = 9;
            public const float FontSizeTitle = 11;
            public const float FontSizeHeading = 14;
            public const float FontSizeDisplay = 18;
        }

        // ============================================================
        // INTERFACE IMPLEMENTATION
        // ============================================================
        public string TipoContenido => "application/pdf";
        public string ExtensionArchivo => ".pdf";

        // ============================================================
        // MÉTODO PRINCIPAL
        // ============================================================
        public byte[] Generar(Cotizacion cotizacion)
        {
            var empresa = cotizacion.Empresa;
            var logoBytes = CargarLogo(empresa.LogoUrl);

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.Letter);
                    page.Margin(35);
                    page.DefaultTextStyle(x => x.FontFamily(DesignTokens.FontFamily));

                    // ─── HEADER ───
                    page.Header().Element(h => ConstruirHeader(h, empresa, cotizacion));

                    // ─── CONTENIDO ───
                    page.Content().Column(content =>
                    {
                        ConstruirInformacionCliente(content, cotizacion);
                        ConstruirTablaEquipos(content, cotizacion);
                        ConstruirTablaInstalaciones(content, cotizacion);
                        ConstruirResumenYTerminos(content, cotizacion);
                        ConstruirFirmas(content, cotizacion);
                    });

                    // ─── FOOTER ───
                    page.Footer().Element(f => ConstruirFooter(f, empresa));
                });
            }).GeneratePdf();
        }

        // ============================================================
        // 1. HEADER - BLOQUE CORPORATIVO
        // ============================================================
        private static void ConstruirHeader(IContainer container, Empresa empresa, Cotizacion cotizacion)
        {
            container.Column(col =>
            {
                col.Item().Row(row =>
                {
                    // ─── Logo y Nombre ───
                    row.RelativeItem().Row(logoRow =>
                    {
                        var logoBytes = CargarLogo(empresa.LogoUrl);
                        if (logoBytes != null)
                        {
                            logoRow.ConstantItem(75).MaxHeight(55).Image(logoBytes).FitArea();
                        }

                        logoRow.RelativeItem().PaddingLeft(12).Column(emp =>
                        {
                            emp.Item().Text(empresa.NombreComercial)
                                .FontSize(DesignTokens.FontSizeDisplay)
                                .Bold()
                                .FontColor(DesignTokens.Primary);

                            if (!string.IsNullOrEmpty(empresa.Eslogan))
                            {
                                emp.Item().Text(empresa.Eslogan)
                                    .FontSize(DesignTokens.FontSizeSmall)
                                    .FontColor(DesignTokens.TextMuted)
                                    .Italic();
                            }
                        });
                    });

                    // ─── Número de Cotización ───
                    row.RelativeItem().AlignRight().Column(cotiz =>
                    {
                        cotiz.Item().Text("COTIZACIÓN")
                            .FontSize(DesignTokens.FontSizeSmall)
                            .Bold()
                            .FontColor(DesignTokens.TextMuted)
                            .LetterSpacing(2);

                        cotiz.Item().Text($"# {cotizacion.NumeroCotizacion}")
                            .FontSize(DesignTokens.FontSizeHeading)
                            .Bold()
                            .FontColor(DesignTokens.Accent);

                        cotiz.Item().PaddingTop(4).Text(text =>
                        {
                            text.Span("Emisión: ").FontSize(DesignTokens.FontSizeSmall).FontColor(DesignTokens.TextMuted);
                            text.Span(cotizacion.FechaCreacion.ToString("dd/MM/yyyy"))
                                .FontSize(DesignTokens.FontSizeSmall)
                                .FontColor(DesignTokens.TextPrimary)
                                .Bold();
                        });

                        cotiz.Item().Text(text =>
                        {
                            text.Span("Vigencia: ").FontSize(DesignTokens.FontSizeSmall).FontColor(DesignTokens.TextMuted);
                            text.Span(cotizacion.FechaVencimiento.ToString("dd/MM/yyyy"))
                                .FontSize(DesignTokens.FontSizeSmall)
                                .FontColor(DesignTokens.Accent)
                                .Bold();
                        });
                    });
                });

                // ─── Línea Divisoria ───
                col.Item().PaddingTop(12).LineHorizontal(1.5f).LineColor(DesignTokens.Primary);
            });
        }

        // ============================================================
        // 2. INFORMACIÓN DEL CLIENTE - TARJETA CON FONDO SUTIL
        // ============================================================
        private static void ConstruirInformacionCliente(ColumnDescriptor content, Cotizacion cotizacion)
        {
            content.Item().PaddingTop(16)
                .Background(DesignTokens.Surface)
                .Padding(16)
                .Row(row =>
                {
                    // ─── Cliente ───
                    row.RelativeItem().Column(col =>
                    {
                        col.Item().Text("CLIENTE")
                            .FontSize(DesignTokens.FontSizeSmall)
                            .Bold()
                            .FontColor(DesignTokens.TextMuted)
                            .LetterSpacing(1);

                        col.Item().PaddingTop(4)
                            .Text(cotizacion.Cliente.Nombre)
                            .FontSize(DesignTokens.FontSizeHeading)
                            .Bold()
                            .FontColor(DesignTokens.TextPrimary);

                        col.Item().PaddingTop(2)
                            .Text(cotizacion.Cliente.ObtenerContactoPrincipal())
                            .FontSize(DesignTokens.FontSizeBody)
                            .FontColor(DesignTokens.TextSecondary);
                    });

                    // ─── Separador ───
                    row.ConstantItem(1)
                        .Height(50)
                        .Background(DesignTokens.Border);

                    // ─── Datos Comerciales ───
                    row.RelativeItem().PaddingLeft(16).Column(col =>
                    {
                        col.Item().Text("DATOS COMERCIALES")
                            .FontSize(DesignTokens.FontSizeSmall)
                            .Bold()
                            .FontColor(DesignTokens.TextMuted)
                            .LetterSpacing(1);

                        col.Item().PaddingTop(4).Text(text =>
                        {
                            text.Span("Ejecutivo: ").FontSize(DesignTokens.FontSizeBody).Bold().FontColor(DesignTokens.TextPrimary);
                            text.Span(cotizacion.Vendedor.NombreCompleto).FontSize(DesignTokens.FontSizeBody).FontColor(DesignTokens.TextSecondary);
                        });

                        col.Item().PaddingTop(2).Text(text =>
                        {
                            text.Span("Área: ").FontSize(DesignTokens.FontSizeBody).Bold().FontColor(DesignTokens.TextPrimary);
                            text.Span($"{cotizacion.AreaMetrosCuadrados:N0} m²").FontSize(DesignTokens.FontSizeBody).FontColor(DesignTokens.TextSecondary);
                        });

                        col.Item().PaddingTop(2).Text(text =>
                        {
                            text.Span("Condiciones: ").FontSize(DesignTokens.FontSizeBody).Bold().FontColor(DesignTokens.TextPrimary);
                            text.Span(cotizacion.CondicionesPago).FontSize(DesignTokens.FontSizeBody).FontColor(DesignTokens.TextSecondary);
                        });
                    });
                });
        }

        // ============================================================
        // 3. TABLA DE EQUIPOS - ZEBRA STRIPING
        // ============================================================
        private static void ConstruirTablaEquipos(ColumnDescriptor content, Cotizacion cotizacion)
        {
            content.Item().PaddingTop(18)
                .Text("EQUIPOS Y MATERIALES")
                .FontSize(DesignTokens.FontSizeTitle)
                .Bold()
                .FontColor(DesignTokens.Primary);

            content.Item().PaddingTop(6);

            content.Item().Table(tabla =>
            {
                // ─── Columnas Flexibles ───
                tabla.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);      // #
                    columns.RelativeColumn(4);        // Descripción (flexible)
                    columns.ConstantColumn(45);       // Cant
                    columns.ConstantColumn(90);       // Precio Unit.
                    columns.ConstantColumn(95);       // Subtotal
                });

                // ─── Header ───
                tabla.Header(header =>
                {
                    header.Cell().Background(DesignTokens.Primary).Padding(6)
                        .Text("#").FontSize(DesignTokens.FontSizeSmall).Bold().FontColor(Colors.White);

                    header.Cell().Background(DesignTokens.Primary).Padding(6)
                        .Text("DESCRIPCIÓN").FontSize(DesignTokens.FontSizeSmall).Bold().FontColor(Colors.White);

                    header.Cell().Background(DesignTokens.Primary).Padding(6).AlignCenter()
                        .Text("CANT").FontSize(DesignTokens.FontSizeSmall).Bold().FontColor(Colors.White);

                    header.Cell().Background(DesignTokens.Primary).Padding(6).AlignRight()
                        .Text("PRECIO UNIT.").FontSize(DesignTokens.FontSizeSmall).Bold().FontColor(Colors.White);

                    header.Cell().Background(DesignTokens.Primary).Padding(6).AlignRight()
                        .Text("SUBTOTAL").FontSize(DesignTokens.FontSizeSmall).Bold().FontColor(Colors.White);
                });

                // ─── Filas con Zebra Striping ───
                int index = 1;
                foreach (var item in cotizacion.ItemsEquipos)
                {
                    var bgColor = (index % 2 == 0) ? DesignTokens.SurfaceHover : Colors.White;

                    tabla.Cell().Background(bgColor).Padding(5)
                        .Text(index.ToString()).FontSize(DesignTokens.FontSizeBody).FontColor(DesignTokens.TextMuted);

                    tabla.Cell().Background(bgColor).Padding(5)
                        .Text($"{item.Equipo.Marca} {item.Equipo.Modelo}")
                        .FontSize(DesignTokens.FontSizeBody)
                        .FontColor(DesignTokens.TextPrimary);

                    tabla.Cell().Background(bgColor).Padding(5).AlignCenter()
                        .Text(item.Cantidad.ToString()).FontSize(DesignTokens.FontSizeBody).FontColor(DesignTokens.TextPrimary);

                    tabla.Cell().Background(bgColor).Padding(5).AlignRight()
                        .Text($"${item.PrecioUnitario.Monto:N2}").FontSize(DesignTokens.FontSizeBody).FontColor(DesignTokens.TextSecondary);

                    tabla.Cell().Background(bgColor).Padding(5).AlignRight()
                        .Text($"${item.Subtotal.Monto:N2}").FontSize(DesignTokens.FontSizeBody).Bold().FontColor(DesignTokens.TextPrimary);

                    index++;
                }
            });
        }

        // ============================================================
        // 4. TABLA DE INSTALACIONES - ZEBRA STRIPING
        // ============================================================
        private static void ConstruirTablaInstalaciones(ColumnDescriptor content, Cotizacion cotizacion)
        {
            if (!cotizacion.ItemsInstalacion.Any()) return;

            content.Item().PaddingTop(15)
                .Text("SERVICIOS E INSTALACIONES")
                .FontSize(DesignTokens.FontSizeTitle)
                .Bold()
                .FontColor(DesignTokens.Primary);

            content.Item().PaddingTop(6);

            content.Item().Table(tabla =>
            {
                // ─── Columnas Flexibles ───
                tabla.ColumnsDefinition(columns =>
                {
                    columns.ConstantColumn(30);       // #
                    columns.RelativeColumn(2);         // Concepto
                    columns.RelativeColumn(3);         // Descripción (flexible)
                    columns.ConstantColumn(45);        // Cant
                    columns.ConstantColumn(85);        // Costo Unit.
                    columns.ConstantColumn(90);        // Subtotal
                });

                // ─── Header ───
                tabla.Header(header =>
                {
                    header.Cell().Background(DesignTokens.PrimaryLight).Padding(6)
                        .Text("#").FontSize(DesignTokens.FontSizeSmall).Bold().FontColor(Colors.White);

                    header.Cell().Background(DesignTokens.PrimaryLight).Padding(6)
                        .Text("CONCEPTO").FontSize(DesignTokens.FontSizeSmall).Bold().FontColor(Colors.White);

                    header.Cell().Background(DesignTokens.PrimaryLight).Padding(6)
                        .Text("DESCRIPCIÓN").FontSize(DesignTokens.FontSizeSmall).Bold().FontColor(Colors.White);

                    header.Cell().Background(DesignTokens.PrimaryLight).Padding(6).AlignCenter()
                        .Text("CANT").FontSize(DesignTokens.FontSizeSmall).Bold().FontColor(Colors.White);

                    header.Cell().Background(DesignTokens.PrimaryLight).Padding(6).AlignRight()
                        .Text("COSTO UNIT.").FontSize(DesignTokens.FontSizeSmall).Bold().FontColor(Colors.White);

                    header.Cell().Background(DesignTokens.PrimaryLight).Padding(6).AlignRight()
                        .Text("SUBTOTAL").FontSize(DesignTokens.FontSizeSmall).Bold().FontColor(Colors.White);
                });

                // ─── Filas con Zebra Striping ───
                int index = 1;
                foreach (var inst in cotizacion.ItemsInstalacion)
                {
                    var bgColor = (index % 2 == 0) ? DesignTokens.SurfaceHover : Colors.White;
                    var descripcion = !string.IsNullOrWhiteSpace(inst.Descripcion)
                        ? inst.Descripcion
                        : (inst.Instalacion?.Descripcion ?? "-");

                    tabla.Cell().Background(bgColor).Padding(5)
                        .Text(index.ToString()).FontSize(DesignTokens.FontSizeBody).FontColor(DesignTokens.TextMuted);

                    tabla.Cell().Background(bgColor).Padding(5)
                        .Text(inst.Concepto).FontSize(DesignTokens.FontSizeBody).FontColor(DesignTokens.TextPrimary);

                    tabla.Cell().Background(bgColor).Padding(5)
                        .Text(descripcion).FontSize(DesignTokens.FontSizeSmall).FontColor(DesignTokens.TextSecondary);

                    tabla.Cell().Background(bgColor).Padding(5).AlignCenter()
                        .Text(inst.Cantidad.ToString()).FontSize(DesignTokens.FontSizeBody).FontColor(DesignTokens.TextPrimary);

                    tabla.Cell().Background(bgColor).Padding(5).AlignRight()
                        .Text($"${inst.CostoUnitario.Monto:N2}").FontSize(DesignTokens.FontSizeBody).FontColor(DesignTokens.TextSecondary);

                    tabla.Cell().Background(bgColor).Padding(5).AlignRight()
                        .Text($"${inst.Subtotal.Monto:N2}").FontSize(DesignTokens.FontSizeBody).Bold().FontColor(DesignTokens.TextPrimary);

                    index++;
                }
            });
        }

        // ============================================================
        // 5. RESUMEN FINANCIERO + TÉRMINOS (DISEÑO EN DOS COLUMNAS)
        // ============================================================
        private static void ConstruirResumenYTerminos(ColumnDescriptor content, Cotizacion cotizacion)
        {
            content.Item().PaddingTop(18).Row(row =>
            {
                // ─── Columna Izquierda: Términos y Condiciones ───
                row.RelativeItem().PaddingRight(20).Column(terms =>
                {
                    terms.Item().Text("TÉRMINOS Y CONDICIONES")
                        .FontSize(DesignTokens.FontSizeSmall)
                        .Bold()
                        .FontColor(DesignTokens.TextPrimary)
                        .LetterSpacing(1);

                    terms.Item().PaddingTop(4).Text(text =>
                    {
                        text.Span("• ").FontSize(DesignTokens.FontSizeSmall).FontColor(DesignTokens.Accent);
                        text.Span("Garantía: ").FontSize(DesignTokens.FontSizeSmall).Bold().FontColor(DesignTokens.TextPrimary);
                        text.Span("1 año en partes y 5 años en compresor").FontSize(DesignTokens.FontSizeSmall).FontColor(DesignTokens.TextSecondary);
                    });

                    terms.Item().PaddingTop(2).Text(text =>
                    {
                        text.Span("• ").FontSize(DesignTokens.FontSizeSmall).FontColor(DesignTokens.Accent);
                        text.Span("Vigencia: ").FontSize(DesignTokens.FontSizeSmall).Bold().FontColor(DesignTokens.TextPrimary);
                        text.Span("30 días naturales a partir de la emisión").FontSize(DesignTokens.FontSizeSmall).FontColor(DesignTokens.TextSecondary);
                    });

                    terms.Item().PaddingTop(2).Text(text =>
                    {
                        text.Span("• ").FontSize(DesignTokens.FontSizeSmall).FontColor(DesignTokens.Accent);
                        text.Span("Disponibilidad: ").FontSize(DesignTokens.FontSizeSmall).Bold().FontColor(DesignTokens.TextPrimary);
                        text.Span("Sujeto a inventario al momento del anticipo").FontSize(DesignTokens.FontSizeSmall).FontColor(DesignTokens.TextSecondary);
                    });

                    terms.Item().PaddingTop(2).Text(text =>
                    {
                        text.Span("• ").FontSize(DesignTokens.FontSizeSmall).FontColor(DesignTokens.Accent);
                        text.Span("IVA: ").FontSize(DesignTokens.FontSizeSmall).Bold().FontColor(DesignTokens.TextPrimary);
                        text.Span("16% incluido en el total").FontSize(DesignTokens.FontSizeSmall).FontColor(DesignTokens.TextSecondary);
                    });

                    // Alerta de Autorización
                    if (cotizacion.RequiereAutorizacion)
                    {
                        terms.Item().PaddingTop(8)
                            .Background(ParseColor("#FEF2F2"))
                            .Padding(8)
                            .Border(1)
                            .BorderColor(ParseColor("#FCA5A5"))
                            .Text("REQUIERE AUTORIZACIÓN DE DIRECCIÓN")
                            .FontSize(DesignTokens.FontSizeSmall)
                            .Bold()
                            .FontColor(DesignTokens.Accent);
                    }
                });

                // ─── Columna Derecha: Totales (Fondo Sutil) ───
                row.ConstantItem(260)
                    .Background(DesignTokens.Surface)
                    .Padding(16)
                    .Column(totales =>
                    {
                        totales.Item().Text("RESUMEN DE COTIZACIÓN")
                            .FontSize(DesignTokens.FontSizeSmall)
                            .Bold()
                            .FontColor(DesignTokens.TextMuted)
                            .LetterSpacing(1)
                            .AlignCenter();

                        totales.Item().PaddingTop(8).Row(r =>
                        {
                            r.RelativeItem().Text("Subtotal:")
                                .FontSize(DesignTokens.FontSizeBody)
                                .FontColor(DesignTokens.TextSecondary);
                            r.RelativeItem().AlignRight()
                                .Text($"$ {cotizacion.Subtotal.Monto:N2}")
                                .FontSize(DesignTokens.FontSizeBody)
                                .FontColor(DesignTokens.TextPrimary);
                        });

                        totales.Item().PaddingTop(2).Row(r =>
                        {
                            r.RelativeItem().Text("IVA (16%):")
                                .FontSize(DesignTokens.FontSizeBody)
                                .FontColor(DesignTokens.TextSecondary);
                            r.RelativeItem().AlignRight()
                                .Text($"$ {cotizacion.Iva.Monto:N2}")
                                .FontSize(DesignTokens.FontSizeBody)
                                .FontColor(DesignTokens.TextPrimary);
                        });

                        totales.Item().PaddingVertical(6)
                            .LineHorizontal(1)
                            .LineColor(DesignTokens.Border);

                        totales.Item().Row(r =>
                        {
                            r.RelativeItem().Text("TOTAL:")
                                .FontSize(DesignTokens.FontSizeHeading)
                                .Bold()
                                .FontColor(DesignTokens.Primary);
                            r.RelativeItem().AlignRight()
                                .Text($"$ {cotizacion.Total.Monto:N2}")
                                .FontSize(DesignTokens.FontSizeHeading)
                                .Bold()
                                .FontColor(DesignTokens.Accent);
                        });

                        totales.Item().PaddingTop(4)
                            .Text("Válido por 30 días naturales")
                            .FontSize(DesignTokens.FontSizeSmall - 1)
                            .FontColor(DesignTokens.TextMuted)
                            .Italic()
                            .AlignCenter();
                    });
            });
        }

        // ============================================================
        // 6. FIRMAS
        // ============================================================
        private static void ConstruirFirmas(ColumnDescriptor content, Cotizacion cotizacion)
        {
            content.Item().PaddingTop(40).Row(row =>
            {
                row.RelativeItem().AlignCenter().Column(col =>
                {
                    col.Item().Width(200).LineHorizontal(1).LineColor(DesignTokens.Border);
                    col.Item().PaddingTop(4)
                        .Text("Cliente (Nombre y Firma)")
                        .FontSize(DesignTokens.FontSizeBody)
                        .FontColor(DesignTokens.TextMuted);
                });

                row.RelativeItem().AlignCenter().Column(col =>
                {
                    col.Item().Width(200).LineHorizontal(1).LineColor(DesignTokens.Border);
                    col.Item().PaddingTop(4)
                        .Text(cotizacion.Vendedor.NombreCompleto)
                        .FontSize(DesignTokens.FontSizeBody)
                        .Bold()
                        .FontColor(DesignTokens.TextPrimary);
                    col.Item()
                        .Text("Representante Comercial")
                        .FontSize(DesignTokens.FontSizeSmall)
                        .FontColor(DesignTokens.TextMuted);
                });
            });
        }

        // ============================================================
        // 7. FOOTER
        // ============================================================
        private static void ConstruirFooter(IContainer container, Empresa empresa)
        {
            container.Column(col =>
            {
                col.Item().LineHorizontal(1).LineColor(DesignTokens.Border);

                col.Item().PaddingTop(6).Row(row =>
                {
                    row.RelativeItem().Text(
                        "Periférico Adolfo López Mateos No. 4293, Int P3-300, Tlalpan, CDMX 14210"
                    ).FontSize(DesignTokens.FontSizeSmall - 1)
                     .FontColor(DesignTokens.TextMuted);

                    row.ConstantItem(100).AlignRight().Text(text =>
                    {
                        text.Span("Página ").FontSize(DesignTokens.FontSizeSmall - 1).FontColor(DesignTokens.TextMuted);
                        text.CurrentPageNumber()
                            .FontSize(DesignTokens.FontSizeSmall - 1)
                            .Bold()
                            .FontColor(DesignTokens.Primary);
                        text.Span(" de ").FontSize(DesignTokens.FontSizeSmall - 1).FontColor(DesignTokens.TextMuted);
                        text.TotalPages()
                            .FontSize(DesignTokens.FontSizeSmall - 1)
                            .Bold()
                            .FontColor(DesignTokens.Primary);
                    });
                });
            });
        }

        // ============================================================
        // 8. UTILIDADES
        // ============================================================
        private static byte[]? CargarLogo(string? logoUrl)
        {
            if (string.IsNullOrEmpty(logoUrl)) return null;

            try
            {
                var logoPath = Path.Combine(
                    Directory.GetCurrentDirectory(),
                    "wwwroot",
                    logoUrl.TrimStart('/')
                );

                if (File.Exists(logoPath))
                {
                    return File.ReadAllBytes(logoPath);
                }
            }
            catch
            {
                // Log silencioso - el logo no es crítico
            }

            return null;
        }

        private static Color ParseColor(string hex)
        {
            if (string.IsNullOrEmpty(hex)) return Color.FromRGB(15, 23, 42);
            if (hex.StartsWith("#")) hex = hex.Substring(1);

            if (hex.Length == 6)
            {
                try
                {
                    var r = Convert.ToByte(hex.Substring(0, 2), 16);
                    var g = Convert.ToByte(hex.Substring(2, 2), 16);
                    var b = Convert.ToByte(hex.Substring(4, 2), 16);
                    return Color.FromRGB(r, g, b);
                }
                catch { /* Ignorar error de parseo */ }
            }

            return Color.FromRGB(15, 23, 42);
        }
    }
}   