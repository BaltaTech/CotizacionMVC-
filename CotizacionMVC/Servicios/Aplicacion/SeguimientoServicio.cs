using CotizacionMVC.Data;
using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Models.Enums;
using CotizacionMVC.Servicios.Aplicacion.Dtos.Seguimientos;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using CotizacionMVC.Servicios.Infraestructura;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Servicios.Aplicacion
{
    public class SeguimientoServicio : ISeguimientoServicio
    {
        private readonly ISeguimientoRepository _seguimientoRepo;
        private readonly ICotizacionRepository _cotizacionRepo;
        private readonly IClienteRepository _clienteRepo;
        private readonly NotificacionServicio _notificacionServicio;
        private readonly ApplicationDbContext _context;

        public SeguimientoServicio(
            ISeguimientoRepository seguimientoRepo,
            ICotizacionRepository cotizacionRepo,
            IClienteRepository clienteRepo,
            NotificacionServicio notificacionServicio,
            ApplicationDbContext context)
        {
            _seguimientoRepo = seguimientoRepo;
            _cotizacionRepo = cotizacionRepo;
            _clienteRepo = clienteRepo;
            _notificacionServicio = notificacionServicio;
            _context = context;
        }

        public async Task<SeguimientoListaDto> RegistrarSeguimientoAsync(CrearSeguimientoDto dto)
        {
            if (!dto.LeadId.HasValue && !dto.CotizacionId.HasValue)
                throw new ArgumentException("Debe especificar un Lead o una Cotización");

            var medioContacto = (MedioContacto)dto.MedioContacto;
            var resultado = (ResultadoSeguimiento)dto.Resultado;
            var etapaNegociacion = dto.EtapaNegociacion.HasValue
                ? (EtapaNegociacion)dto.EtapaNegociacion.Value
                : (EtapaNegociacion?)null;

            Lead? lead = null;
            Cotizacion? cotizacion = null;
            Usuario? vendedor = null;

            if (dto.LeadId.HasValue)
            {
                lead = await _context.Leads
                    .Include(l => l.Empresa)
                    .FirstOrDefaultAsync(l => l.Id == dto.LeadId.Value);

                if (lead == null)
                    throw new ArgumentException("Lead no encontrado");

                if (!lead.PuedeRecibirSeguimientos())
                    throw new InvalidOperationException("Este lead está en un estado terminal");

                if (lead.VendedorAsignadoId.HasValue && lead.VendedorAsignadoId.Value != dto.VendedorId)
                    throw new UnauthorizedAccessException("No tienes permiso para este lead");
            }

            if (dto.CotizacionId.HasValue)
            {
                cotizacion = await _context.Cotizaciones
                    .Include(c => c.Empresa)
                    .Include(c => c.Cliente)
                    .FirstOrDefaultAsync(c => c.Id == dto.CotizacionId.Value);

                if (cotizacion == null)
                    throw new ArgumentException("Cotización no encontrada");

                if (cotizacion.VendedorId != dto.VendedorId)
                    throw new UnauthorizedAccessException("No tienes permiso para esta cotización");

                if (cotizacion.Estado == EstadoCotizacion.Cerrada || cotizacion.Estado == EstadoCotizacion.Perdida)
                    throw new InvalidOperationException("Esta cotización está cerrada");
            }

            vendedor = await _context.Users.FirstOrDefaultAsync(u => u.Id == dto.VendedorId);
            if (vendedor == null)
                throw new ArgumentException("Vendedor no encontrado");

            Seguimiento seguimiento;
            if (lead != null && cotizacion != null)
            {
                seguimiento = new Seguimiento(lead, cotizacion, vendedor, dto.FechaContacto,
                    medioContacto, resultado, dto.Notas, dto.ProximoContacto);
            }
            else if (lead != null)
            {
                seguimiento = new Seguimiento(lead, vendedor, dto.FechaContacto,
                    medioContacto, resultado, dto.Notas, dto.ProximoContacto);
            }
            else
            {
                seguimiento = new Seguimiento(cotizacion!, vendedor, dto.FechaContacto,
                    medioContacto, resultado, dto.Notas, dto.ProximoContacto);
            }

            await _seguimientoRepo.AddAsync(seguimiento);
            await AplicarTransicionesAsync(lead, cotizacion, resultado, etapaNegociacion);
            await _seguimientoRepo.SaveChangesAsync();

            if (dto.ProximoContacto.HasValue)
            {
                await _notificacionServicio.EnviarNotificacionAsync(
                    dto.VendedorId.ToString(),
                    "Recordatorio de Seguimiento",
                    $"Tienes un seguimiento agendado para el {dto.ProximoContacto.Value:dd/MM/yyyy HH:mm}",
                    "warning");
            }

            await _notificacionServicio.EnviarNotificacionAsync(
                dto.VendedorId.ToString(),
                "Dashboard Actualizado",
                "Se ha registrado un nuevo seguimiento",
                "info");

            return MapearADto(seguimiento);
        }

        private async Task AplicarTransicionesAsync(Lead? lead, Cotizacion? cotizacion,
            ResultadoSeguimiento resultado, EtapaNegociacion? nuevaEtapa)
        {
            if (!nuevaEtapa.HasValue) return;

            if (lead != null)
            {
                lead.ActualizarEtapa(nuevaEtapa.Value);
                lead.RegistrarActividad(DateTime.UtcNow);
            }

            if (cotizacion != null)
                cotizacion.ActualizarEtapa(nuevaEtapa.Value);

            SincronizarEnumsViejos(lead, cotizacion, nuevaEtapa.Value);

            if (nuevaEtapa.Value == EtapaNegociacion.CotizacionEnviada && cotizacion?.Cliente != null)
                await NotificarRecepcionCotizadoAsync(cotizacion);

            if (nuevaEtapa.Value == EtapaNegociacion.Cerrada)
                await NotificarVentaCerrada(lead, cotizacion);
        }

        private void SincronizarEnumsViejos(Lead? lead, Cotizacion? cotizacion, EtapaNegociacion etapa)
        {
            switch (etapa)
            {
                case EtapaNegociacion.SinContactar:
                    if (lead != null) lead.ActualizarCategoria(CategoriaLead.SinContactar);
                    break;

                case EtapaNegociacion.ContactoInicial:
                    if (lead != null) lead.ActualizarCategoria(CategoriaLead.Contactado);
                    if (lead != null && lead.Estado == EstadoCliente.Asignado) lead.MarcarContactado();
                    break;

                case EtapaNegociacion.InformacionSolicitada:
                    if (lead != null) lead.ActualizarCategoria(CategoriaLead.Calificado);
                    if (cotizacion != null) cotizacion.CambiarEstado(EstadoCotizacion.InformacionSolicitada);
                    break;

                case EtapaNegociacion.CotizacionEnviada:
                    if (lead != null) lead.ActualizarCategoria(CategoriaLead.Cotizando);
                    if (lead != null) lead.MarcarCotizado();
                    if (cotizacion != null) cotizacion.CambiarEstado(EstadoCotizacion.CotizacionEnviada);
                    break;

                case EtapaNegociacion.PreguntaInstalacion:
                    if (cotizacion != null) cotizacion.CambiarEstado(EstadoCotizacion.PreguntaInstalacion);
                    break;

                case EtapaNegociacion.NegociandoPrecio:
                    if (cotizacion != null) cotizacion.CambiarEstado(EstadoCotizacion.NegociandoPrecio);
                    break;

                case EtapaNegociacion.FechaTentativa:
                    if (cotizacion != null) cotizacion.CambiarEstado(EstadoCotizacion.FechaTentativa);
                    break;

                case EtapaNegociacion.CotizacionFirmada:
                    if (cotizacion != null) cotizacion.CambiarEstado(EstadoCotizacion.Aceptada);
                    break;

                case EtapaNegociacion.AnticipoRecibido:
                    if (cotizacion != null) cotizacion.CambiarEstado(EstadoCotizacion.PagoAnticipo);
                    break;

                case EtapaNegociacion.Cerrada:
                    if (lead != null) lead.MarcarComoConvertido();
                    if (cotizacion != null) cotizacion.CambiarEstado(EstadoCotizacion.Cerrada);
                    break;

                case EtapaNegociacion.Perdida:
                    if (lead != null) lead.MarcarComoNoInteresado();
                    if (cotizacion != null) cotizacion.CambiarEstado(EstadoCotizacion.Perdida);
                    break;
            }
        }

        private async Task NotificarRecepcionCotizadoAsync(Cotizacion cotizacion)
        {
            var usuarios = await _context.Users.Where(u => u.Activo).ToListAsync();
            var recepcion = usuarios.Where(u =>
                _context.UserRoles.Any(ur => ur.UserId == u.Id)).ToList();

            foreach (var r in recepcion)
            {
                await _notificacionServicio.EnviarNotificacionAsync(
                    r.Id.ToString(),
                    "Cliente Cotizado",
                    $"Cliente {cotizacion.Cliente?.Nombre ?? "N/A"} cotizado. Folio: {cotizacion.NumeroCotizacion}",
                    "success");
            }
        }

        private async Task NotificarVentaCerrada(Lead? lead, Cotizacion? cotizacion)
        {
            var nombre = lead?.NombreContacto ?? cotizacion?.Cliente?.Nombre ?? "Cliente";
            var monto = cotizacion?.Total?.Monto ?? 0;

            var usuarios = await _context.Users.Where(u => u.Activo).ToListAsync();
            foreach (var u in usuarios)
            {
                await _notificacionServicio.EnviarNotificacionAsync(
                    u.Id.ToString(),
                    "🎉 Venta Cerrada",
                    $"Cliente: {nombre} | Monto: ${monto:N2}",
                    "success");
            }
        }

        public async Task<IReadOnlyList<SeguimientoListaDto>> ObtenerPorLeadAsync(Guid leadId)
        {
            var seguimientos = await _seguimientoRepo.GetByLeadIdAsync(leadId);
            return seguimientos.Select(MapearADto).ToList();
        }

        public async Task<IReadOnlyList<SeguimientoListaDto>> ObtenerPorCotizacionAsync(Guid cotizacionId)
        {
            var seguimientos = await _seguimientoRepo.GetByCotizacionIdAsync(cotizacionId);
            return seguimientos.Select(MapearADto).ToList();
        }

        public async Task<IReadOnlyList<SeguimientoListaDto>> ObtenerPorVendedorAsync(Guid vendedorId)
        {
            var seguimientos = await _seguimientoRepo.GetByVendedorIdAsync(vendedorId);
            return seguimientos.Select(MapearADto).ToList();
        }

        public async Task<DashboardVendedorDto> ObtenerDashboardAsync(Guid vendedorId)
        {
            var hoy = DateTime.UtcNow.Date;
            var inicioMes = new DateTime(hoy.Year, hoy.Month, 1, 0, 0, 0, DateTimeKind.Utc);
            var leadsVendedor = await _context.Leads
                .Where(l => l.VendedorAsignadoId == vendedorId)
                .ToListAsync();

            var cotizacionesActivas = await _context.Cotizaciones
                .Where(c => c.VendedorId == vendedorId
                    && c.Estado != EstadoCotizacion.Cerrada
                    && c.Estado != EstadoCotizacion.Perdida)
                .CountAsync();

            var pendientesHoy = await _seguimientoRepo.GetPendientesHoyAsync(vendedorId);
            var vencidos = await _seguimientoRepo.GetVencidosAsync(vendedorId);
            var realizadosHoy = await _seguimientoRepo.GetCountByVendedorFechaAsync(vendedorId, hoy);

            var vendidasMes = await _context.Cotizaciones
                .Where(c => c.VendedorId == vendedorId
                    && c.Estado == EstadoCotizacion.Cerrada
                    && c.FechaCreacion >= inicioMes)
                .ToListAsync();

            return new DashboardVendedorDto
            {
                LeadsSinContactar = leadsVendedor.Count(l => l.Categoria == CategoriaLead.SinContactar),
                LeadsFriosSinActividad = leadsVendedor.Count(l => l.EstaSinActividad(15)),
                SeguimientosPendientesHoy = pendientesHoy.Count,
                SeguimientosVencidos = vencidos.Count,
                SeguimientosRealizadosHoy = realizadosHoy,
                CotizacionesActivas = cotizacionesActivas,
                LeadsCalificadosSinCotizar = leadsVendedor.Count(l =>
                    l.Categoria == CategoriaLead.Calificado && l.Estado != EstadoCliente.Cotizado),
                RecepcionSinContactar = leadsVendedor.Count(l =>
                    l.OrigenLead == OrigenLead.Recepcion && l.Categoria == CategoriaLead.SinContactar),
                ProspeccionSinContactar = leadsVendedor.Count(l =>
                    l.OrigenLead == OrigenLead.Prospeccion && l.Categoria == CategoriaLead.SinContactar),
                UrgentesSinContactar = leadsVendedor.Count(l =>
                    l.EtapaNegociacion == EtapaNegociacion.SinContactar && l.OrigenLead == OrigenLead.Recepcion),
                EnNegociacion = leadsVendedor.Count(l =>
                    l.EtapaNegociacion >= EtapaNegociacion.NegociandoPrecio && l.EtapaNegociacion < EtapaNegociacion.Cerrada),
                VendidasMes = vendidasMes.Count,
                MontoVendidoMes = vendidasMes.Sum(c => c.Total.Monto),
                ProximosSeguimientos = pendientesHoy.Select(MapearADto).ToList()
            };
        }

        public async Task MarcarRecordatorioEnviadoAsync(Guid seguimientoId)
        {
            var seguimiento = await _seguimientoRepo.GetByIdAsync(seguimientoId);
            if (seguimiento != null)
            {
                seguimiento.MarcarRecordatorioEnviado();
                _seguimientoRepo.Update(seguimiento);
                await _seguimientoRepo.SaveChangesAsync();
            }
        }

        private SeguimientoListaDto MapearADto(Seguimiento s)
        {
            return new SeguimientoListaDto
            {
                Id = s.Id,
                LeadId = s.LeadId,
                CotizacionId = s.CotizacionId,
                FechaContacto = s.FechaContacto,
                MedioContacto = s.MedioContacto.ToString(),
                Resultado = s.Resultado.ToString(),
                Notas = s.Notas,
                ProximoContacto = s.ProximoContacto,
                VendedorNombre = s.Vendedor?.NombreCompleto ?? "",
                LeadNombre = s.Lead?.NombreContacto,
                CotizacionNumero = s.Cotizacion?.NumeroCotizacion,
                Telefono = s.Lead?.Telefono,
                CorreoElectronico = s.Lead?.CorreoElectronico,
                EtapaNegociacion = s.Lead?.EtapaNegociacion?.ToString() ?? s.Cotizacion?.EtapaNegociacion?.ToString(),
                AlcanceVenta = s.Cotizacion?.AlcanceVenta?.ToString(),
                EsDeLead = s.EsDeLead(),
                EsDeCotizacion = s.EsDeCotizacion()
            };
        }

        public async Task<DashboardRecepcionDto> ObtenerDashboardRecepcionAsync()
        {
            var hoy = DateTime.UtcNow.Date;
            var hace48h = hoy.AddDays(-2);
            var hace7d = hoy.AddDays(-7);

            var clientes = await _context.Clientes.ToListAsync();
            var leads = await _context.Leads.ToListAsync();
            var cotizaciones = await _context.Cotizaciones.ToListAsync();
            var usuarios = await _context.Users.ToListAsync();

            var alertas = new List<AlertaRecepcionDto>();

            foreach (var lead in leads.Where(l => l.Estado == EstadoCliente.Asignado && l.FechaAsignacion < hace48h))
            {
                alertas.Add(new AlertaRecepcionDto
                {
                    Tipo = "warning",
                    Mensaje = "Cliente asignado > 48h sin contactar",
                    ClienteNombre = lead.NombreContacto,
                    Folio = lead.Cliente?.Folio ?? "Sin folio",
                    VendedorNombre = usuarios.FirstOrDefault(u => u.Id == lead.VendedorAsignadoId)?.NombreCompleto
                });
            }

            foreach (var lead in leads.Where(l => l.Estado == EstadoCliente.Contactado && l.FechaContacto < hace7d))
            {
                alertas.Add(new AlertaRecepcionDto
                {
                    Tipo = "danger",
                    Mensaje = "Cliente contactado > 7 días sin cotizar",
                    ClienteNombre = lead.NombreContacto,
                    Folio = lead.Cliente?.Folio ?? "Sin folio",
                    VendedorNombre = usuarios.FirstOrDefault(u => u.Id == lead.VendedorAsignadoId)?.NombreCompleto
                });
            }

            return new DashboardRecepcionDto
            {
                NuevosClientesHoy = clientes.Count(c => c.FechaRegistro.Date == hoy),
                ClientesSinAsignar = clientes.Count(c => c.Estado == EstadoCliente.SinAsignar),
                ClientesAsignadosSinContactar = clientes.Count(c => c.Estado == EstadoCliente.Asignado),
                ClientesContactados = clientes.Count(c => c.Estado == EstadoCliente.Contactado),
                ClientesCotizados = clientes.Count(c => c.Estado == EstadoCliente.Cotizado),
                ClientesEnNegociacion = clientes.Count(c => c.Estado == EstadoCliente.EnNegociacion),
                ClientesCerrados = clientes.Count(c => c.Estado == EstadoCliente.Cerrado),
                ClientesPerdidos = clientes.Count(c => c.Estado == EstadoCliente.Perdido),
                CotizadosHoy = clientes.Count(c => c.FechaCotizacion.HasValue && c.FechaCotizacion.Value.Date == hoy),
                Alertas = alertas
            };
        }
    }
}