// Pruebas Unitarias/Servicios/SeguimientoServicioTests.cs
using CotizacionMVC.Data;
using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Models.Enums;
using CotizacionMVC.Models.Valor;
using CotizacionMVC.Servicios.Aplicacion;
using CotizacionMVC.Servicios.Aplicacion.Dtos.Seguimientos;
using CotizacionMVC.Servicios.Infraestructura;
using Microsoft.EntityFrameworkCore;
using Moq;

namespace CotizacionMVC.Tests.Pruebas_Unitarias.Servicios
{
    public class SeguimientoServicioTests
    {
        private readonly Mock<ISeguimientoRepository> _mockSeguimientoRepo;
        private readonly Mock<ICotizacionRepository> _mockCotizacionRepo;
        private readonly Mock<IClienteRepository> _mockClienteRepo;
        private readonly Mock<NotificacionServicio> _mockNotificacion;
        private readonly ApplicationDbContext _context;

        public SeguimientoServicioTests()
        {
            _mockSeguimientoRepo = new Mock<ISeguimientoRepository>();
            _mockCotizacionRepo = new Mock<ICotizacionRepository>();
            _mockClienteRepo = new Mock<IClienteRepository>();
            _mockNotificacion = new Mock<NotificacionServicio>();

            var options = new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
            _context = new ApplicationDbContext(options);
        }

        private SeguimientoServicio CrearServicio()
        {
            return new SeguimientoServicio(
                _mockSeguimientoRepo.Object,
                _mockCotizacionRepo.Object,
                _mockClienteRepo.Object,
                _mockNotificacion.Object,
                _context);
        }

        private Empresa CrearEmpresa()
        {
            return new Empresa("Test", null, "test", false, "MXN", 0, 0, null, null);
        }

        private Usuario CrearVendedor()
        {
            return new Usuario("Vendedor Test", "vendedor@test.com");
        }

        private async Task<(Empresa empresa, Usuario vendedor, Lead lead)> CrearLeadDePrueba()
        {
            var empresa = CrearEmpresa();
            var vendedor = CrearVendedor();
            var lead = new Lead(empresa, "Juan", "5551234567", CategoriaLead.SinContactar, "Prospeccion", OrigenLead.Prospeccion);
            lead.AsignarVendedor(vendedor);

            _context.Empresas.Add(empresa);
            _context.Users.Add(vendedor);
            _context.Leads.Add(lead);
            await _context.SaveChangesAsync();

            return (empresa, vendedor, lead);
        }

        [Fact]
        public async Task RegistrarSeguimiento_SinRespuesta_SinContactarPasaAFrio()
        {
            var (_, vendedor, lead) = await CrearLeadDePrueba();
            var servicio = CrearServicio();

            var dto = new CrearSeguimientoDto
            {
                LeadId = lead.Id,
                VendedorId = vendedor.Id,
                FechaContacto = DateTime.UtcNow.AddHours(-1),
                MedioContacto = (int)MedioContacto.Telefono,
                Resultado = (int)ResultadoSeguimiento.SinRespuesta
            };

            await servicio.RegistrarSeguimientoAsync(dto);

            var leadActualizado = await _context.Leads.FindAsync(lead.Id);
            Assert.Equal(CategoriaLead.Frio, leadActualizado!.Categoria);
        }

        [Fact]
        public async Task RegistrarSeguimiento_ReagendarLlamada_FrioPasaAContactado()
        {
            var (_, vendedor, lead) = await CrearLeadDePrueba();
            lead.ActualizarCategoria(CategoriaLead.Frio);
            lead.MarcarContactado();
            await _context.SaveChangesAsync();

            var servicio = CrearServicio();
            var dto = new CrearSeguimientoDto
            {
                LeadId = lead.Id,
                VendedorId = vendedor.Id,
                FechaContacto = DateTime.UtcNow.AddHours(-1),
                MedioContacto = (int)MedioContacto.Telefono,
                Resultado = (int)ResultadoSeguimiento.ReagendarLlamada
            };

            await servicio.RegistrarSeguimientoAsync(dto);

            var leadActualizado = await _context.Leads.FindAsync(lead.Id);
            Assert.Equal(CategoriaLead.Contactado, leadActualizado!.Categoria);
        }

        [Fact]
        public async Task RegistrarSeguimiento_CotizacionEnviada_CambiaACotizando()
        {
            var (empresa, vendedor, lead) = await CrearLeadDePrueba();
            lead.ActualizarCategoria(CategoriaLead.Calificado);
            await _context.SaveChangesAsync();

            var contacto = new Contacto("5550000000", null, null, "Cliente");
            var cliente = new Cliente("Cliente Test", contacto);
            var cotizacion = new Cotizacion("COT-001", cliente, empresa, vendedor, 50m, "Contado");
            _context.Clientes.Add(cliente);
            _context.Cotizaciones.Add(cotizacion);
            await _context.SaveChangesAsync();

            var servicio = CrearServicio();
            var dto = new CrearSeguimientoDto
            {
                LeadId = lead.Id,
                CotizacionId = cotizacion.Id,
                VendedorId = vendedor.Id,
                FechaContacto = DateTime.UtcNow.AddHours(-1),
                MedioContacto = (int)MedioContacto.Correo,
                Resultado = (int)ResultadoSeguimiento.CotizacionEnviada
            };

            await servicio.RegistrarSeguimientoAsync(dto);

            var leadActualizado = await _context.Leads.FindAsync(lead.Id);
            Assert.Equal(CategoriaLead.Cotizando, leadActualizado!.Categoria);
        }

        [Fact]
        public async Task RegistrarSeguimiento_NoInteresado_MarcaTerminal()
        {
            var (_, vendedor, lead) = await CrearLeadDePrueba();
            var servicio = CrearServicio();
            var dto = new CrearSeguimientoDto
            {
                LeadId = lead.Id,
                VendedorId = vendedor.Id,
                FechaContacto = DateTime.UtcNow.AddHours(-1),
                MedioContacto = (int)MedioContacto.Telefono,
                Resultado = (int)ResultadoSeguimiento.NoInteresado
            };

            await servicio.RegistrarSeguimientoAsync(dto);

            var leadActualizado = await _context.Leads.FindAsync(lead.Id);
            Assert.Equal(CategoriaLead.NoInteresado, leadActualizado!.Categoria);
            Assert.Equal(EstadoCliente.Perdido, leadActualizado.Estado);
        }

        [Fact]
        public async Task RegistrarSeguimiento_Cerrada_MarcaConvertido()
        {
            var (_, vendedor, lead) = await CrearLeadDePrueba();
            var servicio = CrearServicio();
            var dto = new CrearSeguimientoDto
            {
                LeadId = lead.Id,
                VendedorId = vendedor.Id,
                FechaContacto = DateTime.UtcNow.AddHours(-1),
                MedioContacto = (int)MedioContacto.Visita,
                Resultado = (int)ResultadoSeguimiento.Cerrada
            };

            await servicio.RegistrarSeguimientoAsync(dto);

            var leadActualizado = await _context.Leads.FindAsync(lead.Id);
            Assert.Equal(CategoriaLead.Convertido, leadActualizado!.Categoria);
            Assert.Equal(EstadoCliente.Cerrado, leadActualizado.Estado);
        }

        [Fact]
        public async Task RegistrarSeguimiento_LeadTerminal_LanzaExcepcion()
        {
            var (_, vendedor, lead) = await CrearLeadDePrueba();
            lead.MarcarComoNoInteresado();
            await _context.SaveChangesAsync();

            var servicio = CrearServicio();
            var dto = new CrearSeguimientoDto
            {
                LeadId = lead.Id,
                VendedorId = vendedor.Id,
                FechaContacto = DateTime.UtcNow.AddHours(-1),
                MedioContacto = (int)MedioContacto.Telefono,
                Resultado = (int)ResultadoSeguimiento.ReagendarLlamada
            };

            await Assert.ThrowsAsync<InvalidOperationException>(() =>
                servicio.RegistrarSeguimientoAsync(dto));
        }

        [Fact]
        public async Task RegistrarSeguimiento_SinLeadIdNiCotizacionId_LanzaExcepcion()
        {
            var servicio = CrearServicio();
            var dto = new CrearSeguimientoDto
            {
                VendedorId = Guid.NewGuid(),
                FechaContacto = DateTime.UtcNow,
                MedioContacto = (int)MedioContacto.Telefono,
                Resultado = (int)ResultadoSeguimiento.SinRespuesta
            };

            await Assert.ThrowsAsync<ArgumentException>(() =>
                servicio.RegistrarSeguimientoAsync(dto));
        }

        [Fact]
        public async Task RegistrarSeguimiento_ActualizaUltimoSeguimientoDelLead()
        {
            var (_, vendedor, lead) = await CrearLeadDePrueba();
            var servicio = CrearServicio();
            var dto = new CrearSeguimientoDto
            {
                LeadId = lead.Id,
                VendedorId = vendedor.Id,
                FechaContacto = DateTime.UtcNow.AddHours(-1),
                MedioContacto = (int)MedioContacto.Telefono,
                Resultado = (int)ResultadoSeguimiento.ReagendarLlamada
            };

            await servicio.RegistrarSeguimientoAsync(dto);

            var leadActualizado = await _context.Leads.FindAsync(lead.Id);
            Assert.NotNull(leadActualizado!.UltimoSeguimiento);
            Assert.True((DateTime.UtcNow - leadActualizado.UltimoSeguimiento!.Value).TotalSeconds < 5);
        }
    }
}