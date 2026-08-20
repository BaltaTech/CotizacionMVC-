using CotizacionMVC.Data;
using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Models.Enums;
using CotizacionMVC.Servicios.Aplicacion.Dtos.AdminDashboard;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Servicios.Aplicacion
{
    public class AdminDashboardServicio : IAdminDashboardServicio
    {
        private readonly IClienteRepository _clienteRepository; 
        private readonly ICotizacionRepository _cotizacionRepository; 
        private readonly ILeadRepository _leadRepository; 
        private readonly IUsuarioRepository _usuarioRepository; 
        

        public AdminDashboardServicio(
            IClienteRepository clienteRepository,
            ICotizacionRepository cotizacionRepository,
            ILeadRepository leadRepository,
            IUsuarioRepository usuarioRepository)
        {
            _clienteRepository = clienteRepository;
            _cotizacionRepository = cotizacionRepository;
            _leadRepository = leadRepository;
            _usuarioRepository = usuarioRepository;
        }

        public async Task<AdminDashboardDto> ObtenerDashboardAsync(Guid? empresaId = null)
        {
            var hoy = DateTime.UtcNow.Date;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var clientesNuevosHoy = await _clienteRepository.ObtenerQueryable()
                .Where(c => c.FechaRegistro.Date == hoy)
                .CountAsync();
            var clientesSinAsignar = await _clienteRepository.ObtenerQueryable()
                .Where(c => c.Estado == EstadoCliente.SinAsignar)
                .CountAsync();
            var cotizacionesEnviadasHoy = await _cotizacionRepository.ObtenerQueryable()
                .Where(c => c.Estado == EstadoCotizacion.CotizacionEnviada
                    && c.FechaCreacion.Date == hoy)
                .CountAsync();            
            var cotizacionesActivas = await _cotizacionRepository.ObtenerQueryable()
                .Where(c => c.Estado != EstadoCotizacion.Cerrada
                    && c.Estado != EstadoCotizacion.Perdida)
                .CountAsync();
            var ventasMes = await _cotizacionRepository.ObtenerQueryable()
                .Where(c => c.Estado == EstadoCotizacion.Cerrada
                    && c.FechaCreacion >= inicioMes)
                .ToListAsync();            
            var leadsPerdidosMes = await _leadRepository.ContarPerdidosDesdeAsync(inicioMes);

            var totalClientesAtendidos = await _clienteRepository.ObtenerQueryable()
                .Where(c => c.Estado == EstadoCliente.Cerrado || c.Estado == EstadoCliente.Cotizado)
                .CountAsync();
            var totalClientes = await _clienteRepository.ObtenerQueryable()
                .CountAsync();

            var tasaConversion = totalClientes > 0
                ? Math.Round((decimal)ventasMes.Count / totalClientes * 100, 1)
                : 0;
            var vendedores = await _usuarioRepository.ObtenerActivosAsync();

            var vendedoresMetricas = new List<VendedorMetricasDto>();

            foreach (var vendedor in vendedores)
            {
                var leadsVendedor = await _leadRepository.ObtenerPorVendedorAsync(vendedor.Id);
             
                var cotizacionesVendedor = await _cotizacionRepository.ObtenerQueryable()
                    .Where(c => c.VendedorId == vendedor.Id)
                    .ToListAsync();

                var totalLeadsVendedor = leadsVendedor.Count;
                var sinContactar = leadsVendedor.Count(l => l.Categoria == CategoriaLead.SinContactar);
                var cotActivas = cotizacionesVendedor.Count(c =>
                    c.Estado != EstadoCotizacion.Cerrada && c.Estado != EstadoCotizacion.Perdida);
                var enviadasHoy = cotizacionesVendedor.Count(c =>
                    c.Estado == EstadoCotizacion.CotizacionEnviada && c.FechaCreacion.Date == hoy);
                var vendidasMesVendedor = cotizacionesVendedor.Count(c =>
                    c.Estado == EstadoCotizacion.Cerrada && c.FechaCreacion >= inicioMes);
                var montoVendido = cotizacionesVendedor
                    .Where(c => c.Estado == EstadoCotizacion.Cerrada && c.FechaCreacion >= inicioMes)
                    .Sum(c => c.Total?.Monto ?? 0);
                var conversionVendedor = totalLeadsVendedor > 0
                    ? Math.Round((decimal)vendidasMesVendedor / totalLeadsVendedor * 100, 1)
                    : 0;
                var ultimoSeguimiento = leadsVendedor
                    .Where(l => l.UltimoSeguimiento.HasValue)
                    .Max(l => (DateTime?)l.UltimoSeguimiento);

                vendedoresMetricas.Add(new VendedorMetricasDto
                {
                    VendedorId = vendedor.Id,
                    Nombre = vendedor.NombreCompleto,
                    TotalLeads = totalLeadsVendedor,
                    SinContactar = sinContactar,
                    CotizacionesActivas = cotActivas,
                    EnviadasHoy = enviadasHoy,
                    VendidasMes = vendidasMesVendedor,
                    MontoVendidoMes = montoVendido,
                    TasaConversion = conversionVendedor,
                    UltimoSeguimiento = ultimoSeguimiento
                });
            }

   
            var leadsConEtapa = await _leadRepository.ObtenerTodosAsync();

            var cotizacionesConEtapa = await _cotizacionRepository.ObtenerQueryable()
                .Where(c => c.EtapaNegociacion.HasValue
                    && c.Estado != EstadoCotizacion.Cerrada
                    && c.Estado != EstadoCotizacion.Perdida)
                .ToListAsync();

            var pipeline = new List<PipelineEtapaDto>();

            var etapas = new[] {
                EtapaNegociacion.SinContactar,
                EtapaNegociacion.ContactoInicial,
                EtapaNegociacion.InformacionSolicitada,
                EtapaNegociacion.CotizacionEnviada,
                EtapaNegociacion.PreguntaInstalacion,
                EtapaNegociacion.NegociandoPrecio,
                EtapaNegociacion.FechaTentativa,
                EtapaNegociacion.CotizacionFirmada,
                EtapaNegociacion.AnticipoRecibido
            };

            foreach (var etapa in etapas)
            {
                var cantidadLeads = leadsConEtapa.Count(l => l.EtapaNegociacion == etapa);
                var cantidadCotizaciones = cotizacionesConEtapa.Count(c => c.EtapaNegociacion == etapa);
                var montoEstimado = cotizacionesConEtapa
                    .Where(c => c.EtapaNegociacion == etapa)
                    .Sum(c => c.Total?.Monto ?? 0);

                if (cantidadLeads > 0 || cantidadCotizaciones > 0)
                {
                    pipeline.Add(new PipelineEtapaDto
                    {
                        Etapa = etapa.ToString(),
                        Cantidad = cantidadLeads + cantidadCotizaciones,
                        MontoEstimado = montoEstimado
                    });
                }
            }

            // ========== ALERTAS ==========
            var alertas = new List<string>();

            var hace4Horas = DateTime.UtcNow.AddHours(-4);
            var hace48Horas = hoy.AddDays(-2);

            var urgentesSinContacto = await _leadRepository.ObtenerQueryable()
                .Where(l => l.EtapaNegociacion == EtapaNegociacion.SinContactar
                    && l.OrigenLead == OrigenLead.Recepcion
                    && l.FechaAsignacion < hace4Horas)
                .CountAsync();

            if (urgentesSinContacto > 0)
                alertas.Add($"🚨 {urgentesSinContacto} clientes urgentes sin contacto en más de 4 horas");

            var asignadosSinContacto = await _clienteRepository.ObtenerQueryable()
                .Where(c => c.Estado == EstadoCliente.Asignado
                    && c.FechaAsignacion < hace48Horas)
                .CountAsync();

            if (asignadosSinContacto > 0)
                alertas.Add($"⚠️ {asignadosSinContacto} clientes asignados sin contacto en más de 48h");

            foreach (var v in vendedoresMetricas)
            {
                if (v.SinContactar > 5)
                    alertas.Add($"📋 {v.Nombre} tiene {v.SinContactar} leads sin contactar");
            }
   
            var cotizacionesPorVencer = await _cotizacionRepository.ObtenerQueryable()
                .Where(c => c.Estado != EstadoCotizacion.Cerrada
                    && c.Estado != EstadoCotizacion.Perdida
                    && c.FechaVencimiento <= DateTime.UtcNow.AddDays(3))
                .CountAsync();

            if (cotizacionesPorVencer > 0)
                alertas.Add($"📅 {cotizacionesPorVencer} cotizaciones vencen en los próximos 3 días");

            return new AdminDashboardDto
            {
                ClientesNuevosHoy = clientesNuevosHoy,
                ClientesSinAsignar = clientesSinAsignar,
                CotizacionesEnviadasHoy = cotizacionesEnviadasHoy,
                CotizacionesActivas = cotizacionesActivas,
                VentasMes = ventasMes.Count,
                MontoVendidoMes = ventasMes.Sum(c => c.Total?.Monto ?? 0),
                LeadsPerdidosMes = leadsPerdidosMes,
                TasaConversion = tasaConversion,
                Vendedores = vendedoresMetricas,
                Pipeline = pipeline,
                Alertas = alertas
            };
        }
    }
}