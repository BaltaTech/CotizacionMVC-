using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Models.Enums;
using CotizacionMVC.Models.Valor;
using FluentAssertions;
using Xunit;

namespace CotizacionMVC.Tests.Pruebas_Unitarias.Entidades
{
    public class CotizacionTotalesTests
    {
        // ================================================================
        // HELPERS (Métodos auxiliares para no repetir código)
        // ================================================================

        private Empresa CrearEmpresa(bool esExclusivaTrane = false, string monedaBase = "MXN")
        {
            return new Empresa(
                nombreComercial: "Empresa Test",
                nombreLegal: null,
                slug: "empresa-test",
                esExclusivaTrane: esExclusivaTrane,
                monedaBase: monedaBase,
                utilidadEmpresaPorcentaje: 20m,
                utilidadVendedorPorcentaje: 10m,
                telefonoContacto: "5512345678",
                correoContacto: "test@empresa.com"
            );
        }

        private Cliente CrearCliente()
        {
            var contacto = new Contacto(
                telefono: "5512345678",
                telefonoMovil: null,
                correo: null,
                nombreContacto: "Cliente Test"
            );
            return new Cliente("Cliente Test", contacto);
        }

        private Usuario CrearVendedor()
        {
            return new Usuario("Vendedor Test", "vendedor@test.com");
        }

        private Equipo CrearEquipo(TipoMarca marca, decimal precioBase, string moneda)
        {
            return new Equipo(
                marca: marca,
                modelo: $"Modelo-{Guid.NewGuid():N}".Substring(0, 20),
                capacidadToneladas: 2.5m,
                precioBase: precioBase,
                monedaOriginal: moneda,
                sistema: "Sistema Test",
                modo: "Modo Test",
                descripcion: null
            );
        }

        private Cotizacion CrearCotizacionBase(
            Empresa empresa,
            decimal tipoCambio = 17.43m,
            decimal recargoCiudadPorcentaje = 0m)
        {
            var cliente = CrearCliente();
            var vendedor = CrearVendedor();

            return new Cotizacion(
                numeroCotizacion: "COT-TEST-001",
                cliente: cliente,
                empresa: empresa,
                vendedor: vendedor,
                areaMetrosCuadrados: 100m,
                condicionesPago: "Contado",
                tipoCambio: tipoCambio,
                recargoCiudadPorcentaje: recargoCiudadPorcentaje
            );
        }

        // ================================================================
        // PRUEBA 1: Flujo feliz - Un equipo Trane en USD sin recargo
        // ================================================================

        [Fact]
        public void UnEquipoTraneUSD_SinRecargo_CalculaCorrectamente()
        {
            // ============================================================
            // ARRANGE (Preparar)
            // ============================================================

            // 1. Crear empresa (MXN, utilidad empresa 20%, vendedor 10%)
            var empresa = CrearEmpresa();

            // 2. Crear cotización con tipo de cambio 17.43 y sin recargo
            var cotizacion = CrearCotizacionBase(
                empresa: empresa,
                tipoCambio: 17.43m,
                recargoCiudadPorcentaje: 0m
            );

            // 3. Crear equipo Trane en USD con precio base $1,000
            var equipo = CrearEquipo(
                marca: TipoMarca.Trane,
                precioBase: 1000m,
                moneda: "USD"
            );

            // ============================================================
            // ACT (Ejecutar)
            // ============================================================

            // Agregar el equipo (esto DISPARA RecalcularTotales automáticamente)
            cotizacion.AgregarEquipo(
                equipo: equipo,
                cantidad: 1,
                factorPrecio: 0.31m,
                factorUtilidad: 1.18m,
                descripcionPersonalizada: null
            );

            // ============================================================
            // ASSERT (Verificar)
            // ============================================================

            // Cálculo esperado:
            // Paso 1: Precio USD = 1000 * 0.31 * 1.18 = 365.80 USD
            // Paso 2: Recargo ciudad = 365.80 * 0% = 0 USD
            // Paso 3: Total equipos USD = 365.80 USD
            // Paso 4: Convertir a MXN = 365.80 * 17.43 = 6,375.894 → 6,375.89 MXN
            // Paso 5: Instalaciones = 0 MXN
            // Paso 6: Subtotal = 6,375.89 MXN
            // Paso 7: IVA = 6,375.89 * 0.16 = 1,020.1424 → 1,020.14 MXN
            // Paso 8: Total = 6,375.89 + 1,020.14 = 7,396.03 MXN
            // Paso 9: Autorización = 6,375.89 < 500,000 → false

            var subtotalEsperado = 6375.89m;
            var ivaEsperado = 1020.14m;
            var totalEsperado = 7396.03m;

            // Verificar Subtotal
            cotizacion.Subtotal.Monto.Should().BeApproximately(subtotalEsperado, 0.01m);
            cotizacion.Subtotal.Moneda.Should().Be("MXN");

            // Verificar IVA
            cotizacion.Iva.Monto.Should().BeApproximately(ivaEsperado, 0.01m);
            cotizacion.Iva.Moneda.Should().Be("MXN");

            // Verificar Total
            cotizacion.Total.Monto.Should().BeApproximately(totalEsperado, 0.01m);
            cotizacion.Total.Moneda.Should().Be("MXN");

            // Verificar Recargo Ciudad (debe ser 0)
            cotizacion.RecargoCiudad.Monto.Should().Be(0m);
            cotizacion.RecargoCiudad.Moneda.Should().Be("USD");

            // Verificar que NO requiere autorización
            cotizacion.RequiereAutorizacion.Should().BeFalse();

            // Verificar que los items se agregaron correctamente
            cotizacion.ItemsEquipos.Should().HaveCount(1);
            cotizacion.ItemsEquipos.First().Cantidad.Should().Be(1);
            cotizacion.ItemsEquipos.First().Equipo.Marca.Should().Be(TipoMarca.Trane);

            // Verificar que no hay instalaciones
            cotizacion.ItemsInstalacion.Should().BeEmpty();
        }
    }
}