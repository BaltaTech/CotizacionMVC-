using CotizacionMVC.Models.Entidades;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace CotizacionMVC.Servicios
{
    public class PdfCotizacion : IDocumento
    {
        public string TipoContenido => "application/pdf";
        public string ExtensionArchivo => ".pdf";

        public byte[] Generar(Cotizacion cotizacion)
        {
            var empresa = cotizacion.Empresa;

            byte[]? logoBytes = null;
            if (!string.IsNullOrEmpty(empresa.LogoUrl))
            {
                var logoPath = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", empresa.LogoUrl.TrimStart('/'));
                if (File.Exists(logoPath))
                {
                    logoBytes = File.ReadAllBytes(logoPath);
                }
            }

            return Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(35);

                    var colorPrimario = ParseColor(empresa.ColorPrimario ?? "#1A365D");
                    var colorSecundario = ParseColor(empresa.ColorSecundario ?? "#2D3748");
                    var colorGris = ParseColor("#718096");
                    var colorFondoTabla = ParseColor("#F7FAFC");
                    var colorBordeSuave = ParseColor("#E2E8F0");

                    // ─── ENCABEZADO PROFESIONAL ───
                    page.Header().Element(header =>
                    {
                        header.Column(col =>
                        {
                            col.Item().Row(row =>
                            {
                                // Logo (mantiene proporción)
                                if (logoBytes != null)
                                    row.ConstantItem(85).MaxHeight(55).Image(logoBytes).FitArea();

                                // Nombre de empresa y eslogan
                                row.RelativeItem().Column(emp =>
                                {
                                    emp.Item().Text(empresa.NombreComercial)
                                        .FontSize(20).Bold().FontColor(colorPrimario);
                                    if (!string.IsNullOrEmpty(empresa.Eslogan))
                                        emp.Item().Text(empresa.Eslogan)
                                            .FontSize(9).FontColor(colorGris).Italic();
                                  
                                });

                                // Bloque de cotización alineado a la derecha
                                row.ConstantItem(150).Column(numero =>
                                {
                                    numero.Item().AlignRight().Text("COTIZACIÓN")
                                        .FontSize(9).Bold().FontColor(colorGris);
                                    numero.Item().AlignRight().Text(cotizacion.NumeroCotizacion)
                                        .FontSize(20).Bold().FontColor(colorPrimario);
                                    numero.Item().PaddingTop(3).AlignRight().Text($"Fecha: {cotizacion.FechaCreacion:dd/MM/yyyy}")
                                        .FontSize(8).FontColor(colorGris);
                                    numero.Item().AlignRight().Text($"Vence: {cotizacion.FechaVencimiento:dd/MM/yyyy}")
                                        .FontSize(8).FontColor(colorGris);
                                });
                            });

                            // Línea decorativa doble
                            col.Item().PaddingTop(8).LineHorizontal(1.5f).LineColor(colorPrimario);
                            col.Item().PaddingTop(2).LineHorizontal(0.5f).LineColor(colorSecundario);
                        });
                    });

                    // ─── CONTENIDO PRINCIPAL ───
                    page.Content().Column(content =>
                    {
                        // Bloque Cliente / Datos Comerciales
                        content.Item().PaddingTop(15).Row(datos =>
                        {
                            datos.RelativeItem().Column(cliente =>
                            {
                                cliente.Item().Text("CLIENTE").FontSize(9).Bold().FontColor(colorPrimario);
                                cliente.Item().PaddingTop(2).LineHorizontal(1).LineColor(colorPrimario);
                                cliente.Item().PaddingTop(6).Text(cotizacion.Cliente.Nombre)
                                    .FontSize(13).Bold().FontColor(colorSecundario);
                                cliente.Item().Text(cotizacion.Cliente.ObtenerContactoPrincipal())
                                    .FontSize(9).FontColor(colorGris);
                            });

                            datos.ConstantItem(200).Column(vendedor =>
                            {
                                vendedor.Item().Text("DATOS COMERCIALES").FontSize(9).Bold().FontColor(colorPrimario);
                                vendedor.Item().PaddingTop(2).LineHorizontal(1).LineColor(colorPrimario);
                                vendedor.Item().PaddingTop(6).Text($"Vendedor: {cotizacion.Vendedor.NombreCompleto}")
                                    .FontSize(9).FontColor(colorSecundario);
                                vendedor.Item().Text($"Área: {cotizacion.AreaMetrosCuadrados:N0} m²")
                                    .FontSize(9).FontColor(colorSecundario);
                                vendedor.Item().Text($"Condiciones: {cotizacion.CondicionesPago}")
                                    .FontSize(9).FontColor(colorSecundario);
                            });
                        });

                        // Tabla de Equipos
                        content.Item().PaddingTop(20).Text("EQUIPOS COTIZADOS")
                            .FontSize(11).Bold().FontColor(colorPrimario);
                        content.Item().PaddingTop(8);

                        content.Item().Table(tabla =>
                        {
                            tabla.ColumnsDefinition(columns =>
                            {
                                columns.ConstantColumn(50);
                                columns.RelativeColumn();
                                columns.ConstantColumn(95);
                                columns.ConstantColumn(95);
                            });

                            // Encabezado con fondo de color
                            tabla.Header(header =>
                            {
                                header.Cell().Background(colorPrimario).PaddingVertical(6).PaddingHorizontal(5)
                                    .Text("Cant").FontSize(9).Bold().FontColor(Colors.White);
                                header.Cell().Background(colorPrimario).PaddingVertical(6).PaddingHorizontal(5)
                                    .Text("Descripción").FontSize(9).Bold().FontColor(Colors.White);
                                header.Cell().Background(colorPrimario).PaddingVertical(6).PaddingHorizontal(5).AlignRight()
                                    .Text("P. Unitario").FontSize(9).Bold().FontColor(Colors.White);
                                header.Cell().Background(colorPrimario).PaddingVertical(6).PaddingHorizontal(5).AlignRight()
                                    .Text("Subtotal").FontSize(9).Bold().FontColor(Colors.White);
                            });

                            bool alternar = false;
                            foreach (var item in cotizacion.ItemsEquipos)
                            {
                                var colorFila = alternar ? colorFondoTabla : Colors.White;
                                alternar = !alternar;

                                tabla.Cell().Background(colorFila).PaddingVertical(6).PaddingHorizontal(5)
                                    .Text(item.Cantidad.ToString()).FontSize(10);
                                tabla.Cell().Background(colorFila).PaddingVertical(6).PaddingHorizontal(5)
                                    .Text($"{item.Equipo.Marca} {item.Equipo.Modelo}").FontSize(10);
                                tabla.Cell().Background(colorFila).PaddingVertical(6).PaddingHorizontal(5).AlignRight()
                                    .Text($"{item.PrecioUnitario.Monto:N2}").FontSize(10);
                                tabla.Cell().Background(colorFila).PaddingVertical(6).PaddingHorizontal(5).AlignRight()
                                    .Text($"{item.Subtotal.Monto:N2}").FontSize(10).Bold();
                            }
                        });

                        // Tabla de Instalaciones (si existen)
                        if (cotizacion.ItemsInstalacion.Any())
                        {
                            content.Item().PaddingTop(20).Text("SERVICIOS E INSTALACIONES")
                                .FontSize(11).Bold().FontColor(colorPrimario);
                            content.Item().PaddingTop(8);

                            content.Item().Table(tabla =>
                            {
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(80);  // Concepto
                                    columns.RelativeColumn();    // Descripción
                                    columns.ConstantColumn(45);  // Cantidad
                                    columns.ConstantColumn(85);  // Costo Unitario
                                    columns.ConstantColumn(85);  // Subtotal
                                });

                                // Encabezado con fondo
                                tabla.Header(header =>
                                {
                                    header.Cell().Background(colorPrimario).PaddingVertical(6).PaddingHorizontal(5)
                                        .Text("Concepto").FontSize(9).Bold().FontColor(Colors.White);
                                    header.Cell().Background(colorPrimario).PaddingVertical(6).PaddingHorizontal(5)
                                        .Text("Descripción").FontSize(9).Bold().FontColor(Colors.White);
                                    header.Cell().Background(colorPrimario).PaddingVertical(6).PaddingHorizontal(5).AlignCenter()
                                        .Text("Cant").FontSize(9).Bold().FontColor(Colors.White);
                                    header.Cell().Background(colorPrimario).PaddingVertical(6).PaddingHorizontal(5).AlignRight()
                                        .Text("Costo Unit.").FontSize(9).Bold().FontColor(Colors.White);
                                    header.Cell().Background(colorPrimario).PaddingVertical(6).PaddingHorizontal(5).AlignRight()
                                        .Text("Subtotal").FontSize(9).Bold().FontColor(Colors.White);
                                });

                                bool alternar = false;
                                foreach (var inst in cotizacion.ItemsInstalacion)
                                {
                                    var colorFila = alternar ? colorFondoTabla : Colors.White;
                                    alternar = !alternar;

                                    string descripcionMostrar = !string.IsNullOrWhiteSpace(inst.Descripcion)
                                        ? inst.Descripcion
                                        : (inst.Instalacion?.Descripcion ?? "Sin descripción");

                                    tabla.Cell().Background(colorFila).PaddingVertical(6).PaddingHorizontal(5)
                                        .Text(inst.Concepto).FontSize(9).Bold();
                                    tabla.Cell().Background(colorFila).PaddingVertical(6).PaddingHorizontal(5)
                                        .Text(descripcionMostrar).FontSize(9).LineHeight(1.2f);
                                    tabla.Cell().Background(colorFila).PaddingVertical(6).PaddingHorizontal(5).AlignCenter()
                                        .Text(inst.Cantidad.ToString()).FontSize(9);
                                    tabla.Cell().Background(colorFila).PaddingVertical(6).PaddingHorizontal(5).AlignRight()
                                        .Text($"{inst.CostoUnitario.Monto:N2}").FontSize(9);
                                    tabla.Cell().Background(colorFila).PaddingVertical(6).PaddingHorizontal(5).AlignRight()
                                        .Text($"{inst.Subtotal.Monto:N2}").FontSize(9).Bold();
                                }
                            });
                        }

                        // Bloque de totales (con fondo suave y borde redondeado)
                        content.Item().PaddingTop(20).AlignRight().Element(totalesContainer =>
                        {
                            totalesContainer.Column(colTotales =>
                            {
                                colTotales.Item().Background(colorFondoTabla).Border(1).BorderColor(colorBordeSuave)
                                    .Padding(12).Column(inner =>
                                    {
                                        inner.Item().Text($"Subtotal: {cotizacion.Subtotal.Monto:N2}")
                                            .FontSize(10).FontColor(colorSecundario);
                                        inner.Item().Text($"IVA (16%): {cotizacion.Iva.Monto:N2}")
                                            .FontSize(10).FontColor(colorSecundario);
                                        inner.Item().PaddingTop(6).LineHorizontal(1).LineColor(colorPrimario);
                                        inner.Item().PaddingTop(4).Text($"TOTAL: {cotizacion.Total.Monto:N2}")
                                            .FontSize(15).Bold().FontColor(colorPrimario);
                                    });
                            });
                        });

                        // Condiciones de pago y vendedor
                        content.Item().PaddingTop(18).Column(cond =>
                        {
                            cond.Item().Text("CONDICIONES").FontSize(9).Bold().FontColor(colorGris);
                            cond.Item().Text(cotizacion.CondicionesPago).FontSize(9).FontColor(colorSecundario);
                            cond.Item().Text($"Vendedor: {cotizacion.Vendedor.NombreCompleto}").FontSize(9).FontColor(colorSecundario);
                        });

                        // Aviso de autorización
                        if (cotizacion.RequiereAutorizacion)
                        {
                            content.Item().PaddingTop(12).Background(ParseColor("#FFF3CD"))
                                .Border(1).BorderColor(ParseColor("#FFC107"))
                                .Padding(8).Text("⚠ Esta cotización requiere autorización por ser mayor a $500,000 MXN")
                                .FontSize(8.5f).FontColor(ParseColor("#856404"));
                        }
                    });

                    // ─── PIE DE PÁGINA PROFESIONAL ───
                    page.Footer().Element(footer =>
                    {
                        footer.Column(ft =>
                        {
                            ft.Item().PaddingBottom(3).LineHorizontal(0.5f).LineColor(colorBordeSuave);
                            ft.Item().Row(r =>
                            {
                                r.RelativeItem().Text(empresa.NombreComercial)
                                    .FontSize(7.5f).FontColor(colorGris);
                                r.ConstantItem(120).AlignRight().Text(text =>
                                {
                                    text.Span("Pág. ").FontSize(7.5f).FontColor(colorGris);
                                    text.CurrentPageNumber().FontSize(7.5f).FontColor(colorGris);
                                    text.Span(" de ").FontSize(7.5f).FontColor(colorGris);
                                    text.TotalPages().FontSize(7.5f).FontColor(colorGris);
                                });
                            });
                        });
                    });
                });
            }).GeneratePdf();
        }

        private static Color ParseColor(string hex)
        {
            if (string.IsNullOrEmpty(hex)) hex = "#3B82F6";
            if (hex.StartsWith("#")) hex = hex.Substring(1);
            if (hex.Length == 6)
            {
                var r = Convert.ToByte(hex.Substring(0, 2), 16);
                var g = Convert.ToByte(hex.Substring(2, 2), 16);
                var b = Convert.ToByte(hex.Substring(4, 2), 16);
                return Color.FromRGB(r, g, b);
            }
            return Color.FromRGB(59, 130, 246);
        }
    }
}