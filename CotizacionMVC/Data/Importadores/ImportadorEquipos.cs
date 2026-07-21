using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Data.Importadores
{
    public static class ImportadorEquipos
    {
        public static async Task ImportarDesdeCsvAsync(ApplicationDbContext context, string rutaCsv)
        {
            var lineas = await File.ReadAllLinesAsync(rutaCsv);
            int importados = 0;
            int saltados = 0;
            int errores = 0;

            for (int i = 1; i < lineas.Length; i++)
            {
                var linea = lineas[i];
                if (string.IsNullOrWhiteSpace(linea)) continue;

                var columnas = ParsearCsv(linea);
                if (columnas.Length < 7) continue;

                var marcaTexto = columnas[0].Trim();
                var sistema = columnas[1].Trim();
                var modelo = columnas[2].Trim();
                var modo = columnas[3].Trim();
                var descripcion = columnas[4].Trim();
                var capacidadTexto = columnas[5].Trim();
                var precioTexto = columnas[6].Trim();
                var moneda = columnas.Length > 7 ? columnas[7].Trim().ToUpper() : "USD";

                if (string.IsNullOrWhiteSpace(modelo)) continue;
                if (moneda != "MXN" && moneda != "USD") moneda = "USD";

                TipoMarca marca = marcaTexto.ToUpper() switch
                {
                    "TRANE" => TipoMarca.Trane,
                    "YORK" => TipoMarca.York,
                    "CARRIER" => TipoMarca.Carrier,
                    "DAIKIN" => TipoMarca.Daikin,
                    "MITSUBISHI" => TipoMarca.Mitsubishi,
                    "HISENSE" => TipoMarca.Hisense,
                    "TCL" => TipoMarca.TCL,
                    _ => TipoMarca.Otro
                };

                decimal capacidad = 0;
                if (!string.IsNullOrWhiteSpace(capacidadTexto))
                {
                    var partes = capacidadTexto
                        .Replace("T.R", "")
                        .Replace("TR", "")
                        .Replace("TON", "")
                        .Trim();
                    decimal.TryParse(partes,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out capacidad);
                }

                decimal precio = 0;
                if (!string.IsNullOrWhiteSpace(precioTexto))
                {
                    decimal.TryParse(precioTexto,
                        System.Globalization.NumberStyles.Any,
                        System.Globalization.CultureInfo.InvariantCulture,
                        out precio);
                }

                var existe = await context.Equipos.AnyAsync(e => e.Modelo == modelo);
                if (existe)
                {
                    saltados++;
                    continue;
                }

                try
                {
                    var equipo = new Equipo(
                        marca,
                        modelo,
                        capacidad,
                        precio,
                        moneda,
                        sistema,
                        modo,
                        string.IsNullOrWhiteSpace(descripcion) ? null : descripcion
                    );

                    context.Equipos.Add(equipo);
                    importados++;
                }
                catch (Exception ex)
                {
                    errores++;
                    Console.WriteLine($"Error línea {i + 1}: {modelo} - {ex.Message}");
                }
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"Importación completada:");
            Console.WriteLine($"{importados} equipos importados");
            Console.WriteLine($"{saltados} duplicados saltados");
            Console.WriteLine($"{errores} errores");
        }

        private static string[] ParsearCsv(string linea)
        {
            var resultado = new List<string>();
            var enComillas = false;
            var valor = "";

            foreach (var c in linea)
            {
                if (c == '"')
                {
                    enComillas = !enComillas;
                }
                else if (c == ',' && !enComillas)
                {
                    resultado.Add(valor.Trim());
                    valor = "";
                }
                else
                {
                    valor += c;
                }
            }
            resultado.Add(valor.Trim());
            return resultado.ToArray();
        }
    }
}