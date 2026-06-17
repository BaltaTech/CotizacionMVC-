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
                if (System.IO.File.Exists(logoPath))
                {                     
                    logoBytes = System.IO.File.ReadAllBytes(logoPath);
                }
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
                                // 1. Aquí definimos las 5 columnas del PDF (Concepto, Descripción, Cantidad, Costo, Subtotal)
                                tabla.ColumnsDefinition(columns =>
                                {
                                    columns.ConstantColumn(80);   // Concepto
                                    columns.RelativeColumn();     // Descripción (Aquí se expandirá el texto largo)
                                    columns.ConstantColumn(40);   // Cantidad
                                    columns.ConstantColumn(80);   // Costo Unitario
                                    columns.ConstantColumn(80);   // Subtotal
                                });

                                // 2. Aquí dibujamos las 5 cabeceras del PDF
                                tabla.Header(header =>
                                {
                                    header.Cell().BorderBottom(1).BorderColor(colorPrimario).PaddingBottom(5)
                                        .Text("Concepto").FontSize(9).Bold().FontColor(colorPrimario);

                                    header.Cell().BorderBottom(1).BorderColor(colorPrimario).PaddingBottom(5)
                                        .Text("Descripción").FontSize(9).Bold().FontColor(colorPrimario);

                                    header.Cell().BorderBottom(1).BorderColor(colorPrimario).PaddingBottom(5).AlignCenter()
                                        .Text("Cant").FontSize(9).Bold().FontColor(colorPrimario);

                                    header.Cell().BorderBottom(1).BorderColor(colorPrimario).PaddingBottom(5).AlignRight()
                                        .Text("Costo Unit.").FontSize(9).Bold().FontColor(colorPrimario);

                                    header.Cell().BorderBottom(1).BorderColor(colorPrimario).PaddingBottom(5).AlignRight()
                                        .Text("Subtotal").FontSize(9).Bold().FontColor(colorPrimario);
                                });

                                // 3. Aquí vaciamos las 5 celdas con la información por cada fila
                                foreach (var inst in cotizacion.ItemsInstalacion)
                                {
                                    tabla.Cell().PaddingVertical(5)
                                        .Text(inst.Concepto).FontSize(9).Bold();

                                    // Extraemos la descripción de la instalación que antes no se mandaba a llamar
                                    string descripcionMostrar = !string.IsNullOrWhiteSpace(inst.Descripcion)
                                        ? inst.Descripcion
                                        : (inst.Instalacion?.Descripcion ?? "Sin descripción");

                                    tabla.Cell().PaddingVertical(5)
                                        .Text(descripcionMostrar).FontSize(9).LineHeight(1.2f);

                                    tabla.Cell().PaddingVertical(5).AlignCenter()
                                        .Text(inst.Cantidad.ToString()).FontSize(9);

                                    tabla.Cell().PaddingVertical(5).AlignRight()
                                        .Text($"{inst.CostoUnitario.Monto:N2}").FontSize(9);

                                    tabla.Cell().PaddingVertical(5).AlignRight()
                                        .Text($"{inst.Subtotal.Monto:N2}").FontSize(9).Bold();
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
}