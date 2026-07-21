using CotizacionMVC.Models.Entidades;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Data.Importadores
{
    public static class ImportadorInstalaciones
    {
        public static async Task ImportarDesdeCsvAsync(ApplicationDbContext context, string rutaCsv)
        {
            var lineas = await File.ReadAllLinesAsync(rutaCsv);
            int importados = 0;
            int saltados = 0;

            for (int i = 1; i < lineas.Length; i++)
            {
                var linea = lineas[i];
                if (string.IsNullOrWhiteSpace(linea)) continue;

                var columnas = linea.Split(',');

                if (columnas.Length < 4) continue;

                var categoria = columnas[0].Trim();
                var concepto = columnas[1].Trim();
                var descripcion = columnas[2].Trim();
                var costoTexto = columnas[3].Trim();

                if (string.IsNullOrWhiteSpace(concepto)) continue;

                decimal costo = 0;
                decimal.TryParse(costoTexto, out costo);

                var existe = await context.Instalaciones
                    .AnyAsync(ins => ins.Concepto == concepto && ins.Categoria == categoria);

                if (existe)
                {
                    saltados++;
                    continue;
                }

                var instalacion = new Instalacion(
                    concepto,
                    string.IsNullOrWhiteSpace(descripcion) ? null : descripcion,
                    costo,
                    categoria
                );

                context.Instalaciones.Add(instalacion);
                importados++;
            }

            await context.SaveChangesAsync();
            Console.WriteLine($"Instalaciones: {importados} importadas, {saltados} duplicadas saltadas.");
        }
    }
}