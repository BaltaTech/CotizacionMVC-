using CotizacionMVC.Data;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Models.Enums;
using CotizacionMVC.Models.Valor;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;

namespace CotizacionMVC.Tests.Pruebas_Integracion
{
    public class DetectarErrorCotizacionTests
    {
        private ApplicationDbContext CrearContexto()
        {
            var opciones = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(Guid.NewGuid().ToString())
                .Options;
            return new ApplicationDbContext(opciones);
        }

        // ─── PRUEBA 1: ¿La entidad guarda items en memoria? ───
        [Fact]
        public void Cotizacion_AgregarEquipo_LosItemsEstanEnMemoria()
        {
            // Arrange
            var empresa = new Empresa("Test", null, "test", false, "MXN", 30, 5, "5512345678", "a@a.com");
            var cliente = new Cliente("Cliente", new Contacto("5512345678", null, null, null));
            var vendedor = new Usuario("Vendedor", "v@v.com", RolUsuario.Vendedor);
            var equipo = new Equipo(TipoMarca.Trane, "XR14", 2.5m, 1000m, "USD");

            var cotizacion = new Cotizacion("COT-0001", cliente, empresa, vendedor, 100m, "Contado");

            // Act
            cotizacion.AgregarEquipo(equipo, 2, 30, 5, null);

            // Assert
            cotizacion.ItemsEquipos.Should().NotBeEmpty("los items deben estar en la colección en memoria");
            cotizacion.ItemsEquipos.Should().HaveCount(1);
            cotizacion.ItemsEquipos.First().Equipo.Modelo.Should().Be("XR14");
            cotizacion.ItemsEquipos.First().Cantidad.Should().Be(2);
        }

        // ─── PRUEBA 2: ¿EF Core guarda los items en la BD? ───
        [Fact]
        public async Task Cotizacion_GuardarYRecuperar_LosItemsSePersisten()
        {
            // Arrange
            using var contexto = CrearContexto();

            var empresa = new Empresa("Test", null, "test", false, "MXN", 30, 5, "5512345678", "a@a.com");
            var cliente = new Cliente("Cliente", new Contacto("5512345678", null, null, null));
            var vendedor = new Usuario("Vendedor", "v@v.com", RolUsuario.Vendedor);
            var equipo = new Equipo(TipoMarca.Trane, "XR14", 2.5m, 1000m, "USD");

            contexto.Empresas.Add(empresa);
            contexto.Clientes.Add(cliente);
            contexto.Usuarios.Add(vendedor);
            contexto.Equipos.Add(equipo);
            await contexto.SaveChangesAsync();

            var cotizacion = new Cotizacion("COT-0001", cliente, empresa, vendedor, 100m, "Contado");
            cotizacion.AgregarEquipo(equipo, 2, 30, 5, null);
            contexto.Cotizaciones.Add(cotizacion);

            // Act
            await contexto.SaveChangesAsync();

            // Assert
            var cotizacionDb = await contexto.Cotizaciones
                .Include(c => c.ItemsEquipos)
                    .ThenInclude(i => i.Equipo)
                .FirstOrDefaultAsync(c => c.Id == cotizacion.Id);

            cotizacionDb.Should().NotBeNull("la cotización debe existir en la BD");
            cotizacionDb.ItemsEquipos.Should().NotBeEmpty("los items deben persistirse");
            cotizacionDb.ItemsEquipos.Should().HaveCount(1);
        }

        // ─── PRUEBA 3: Simula el JSON que llega del frontend ───
        [Fact]
        public void JsonDelFrontend_Deserializar_LosEquiposSeParseaCorrectamente()
        {
            // Arrange
            string jsonEquipos = "[{\"EquipoId\":\"00000000-0000-0000-0000-000000000001\",\"Cantidad\":2}]";

            // Act
            var listaEquipos = string.IsNullOrEmpty(jsonEquipos)
                ? new List<ItemCotizacionJson>()
                : System.Text.Json.JsonSerializer.Deserialize<List<ItemCotizacionJson>>(jsonEquipos);

            // Assert
            listaEquipos.Should().NotBeNull("el JSON debe deserializarse");
            listaEquipos.Should().NotBeEmpty("debe haber al menos un equipo");
            listaEquipos[0].Cantidad.Should().Be(2);
            listaEquipos[0].EquipoId.Should().NotBe(Guid.Empty);
        }

        // ─── PRUEBA 4: ¿Los totales se recalculan? ───
        [Fact]
        public void Cotizacion_AgregarEquipo_RecalculaTotales()
        {
            // Arrange
            var empresa = new Empresa("Test", null, "test", false, "MXN", 30, 5, "5512345678", "a@a.com");
            var cliente = new Cliente("Cliente", new Contacto("5512345678", null, null, null));
            var vendedor = new Usuario("Vendedor", "v@v.com", RolUsuario.Vendedor);
            var equipo = new Equipo(TipoMarca.Trane, "XR14", 2.5m, 1000m, "USD");

            var cotizacion = new Cotizacion("COT-0001", cliente, empresa, vendedor, 100m, "Contado");

            // Act
            cotizacion.AgregarEquipo(equipo, 2, 30, 5, null);

            // Assert
            cotizacion.Subtotal.Monto.Should().BeGreaterThan(0, "el subtotal debe recalcularse");
            cotizacion.Iva.Monto.Should().BeGreaterThan(0, "el IVA debe recalcularse");
            cotizacion.Total.Monto.Should().BeGreaterThan(0, "el total debe recalcularse");
        }
    }

    public class ItemCotizacionJson
    {
        public Guid EquipoId { get; set; }
        public int Cantidad { get; set; }
    }
}