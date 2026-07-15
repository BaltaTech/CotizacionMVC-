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
            await AplicarTransicionesAsync(lead, cotizacion, resultado);
            await _seguimientoRepo.SaveChangesAsync();

            // Notificar al vendedor si hay próximo contacto agendado
            if (dto.ProximoContacto.HasValue)
            {
                await _notificacionServicio.EnviarNotificacionAsync(
                    dto.VendedorId.ToString(),
                    "📅 Recordatorio de Seguimiento",
                    $"Tienes un seguimiento agendado para el {dto.ProximoContacto.Value:dd/MM/yyyy HH:mm}",
                    "warning");
            }

            // Actualizar dashboard del vendedor en tiempo real
            await _notificacionServicio.EnviarNotificacionAsync(
                dto.VendedorId.ToString(),
                "📊 Dashboard Actualizado",
                "Se ha registrado un nuevo seguimiento",
                "info");

            return MapearADto(seguimiento);
        }
        private async Task AplicarTransicionesAsync(Lead? lead, Cotizacion? cotizacion, ResultadoSeguimiento resultado)
        {
            switch (resultado)
            {
                case ResultadoSeguimiento.SinRespuesta:
                    if (lead != null && lead.Categoria == CategoriaLead.SinContactar)
                        lead.ActualizarCategoria(CategoriaLead.Frio);
                    break;

                case ResultadoSeguimiento.NoInteresado:
                    if (lead != null) lead.MarcarComoNoInteresado();
                    if (cotizacion != null) cotizacion.CambiarEstado(EstadoCotizacion.Perdida);
                    break;

                case ResultadoSeguimiento.ReagendarLlamada:
                    if (lead != null && lead.Categoria == CategoriaLead.Frio)
                        lead.ActualizarCategoria(CategoriaLead.Contactado);
                    if (lead != null && lead.Estado == EstadoCliente.Asignado)
                        lead.MarcarContactado();
                    break;

                case ResultadoSeguimiento.SolicitoVisitaTecnica:
                    if (lead != null) lead.ActualizarCategoria(CategoriaLead.Calificado);
                    if (cotizacion != null) cotizacion.CambiarEstado(EstadoCotizacion.PreguntaInstalacion);
                    break;

                case ResultadoSeguimiento.VisitaTecnicaRealizada:
                case ResultadoSeguimiento.DatosRecabados:
                case ResultadoSeguimiento.CotizacionSolicitada:
                    if (lead != null) lead.ActualizarCategoria(CategoriaLead.Calificado);
                    if (cotizacion != null) cotizacion.CambiarEstado(EstadoCotizacion.InformacionSolicitada);
                    break;

                case ResultadoSeguimiento.CotizacionEnviada:
                    if (lead != null) lead.ActualizarCategoria(CategoriaLead.Cotizando);
                    if (cotizacion != null)
                    {
                        cotizacion.CambiarEstado(EstadoCotizacion.CotizacionEnviada);
                        if (cotizacion.Cliente != null && cotizacion.Cliente.Estado != EstadoCliente.Cotizado)
                        {
                            cotizacion.Cliente.MarcarCotizado();
                            await NotificarRecepcionCotizadoAsync(cotizacion);
                        }
                    }
                    if (lead != null && lead.Estado != EstadoCliente.Cotizado)
                        lead.MarcarCotizado();
                    break;

                case ResultadoSeguimiento.NegociandoPrecio:
                case ResultadoSeguimiento.SolicitandoAlternativa:
                case ResultadoSeguimiento.EvaluandoFinanciamiento:
                    if (cotizacion != null) cotizacion.CambiarEstado(EstadoCotizacion.NegociandoPrecio);
                    break;

                case ResultadoSeguimiento.FechaTentativaInstalacion:
                    if (cotizacion != null) cotizacion.CambiarEstado(EstadoCotizacion.FechaTentativa);
                    break;

                case ResultadoSeguimiento.AnticipoSolicitado:
                    if (cotizacion != null) cotizacion.CambiarEstado(EstadoCotizacion.Aceptada);
                    break;

                case ResultadoSeguimiento.AnticipoRecibido:
                    if (cotizacion != null) cotizacion.CambiarEstado(EstadoCotizacion.PagoAnticipo);
                    break;

                case ResultadoSeguimiento.Cerrada:
                    if (lead != null) lead.MarcarComoConvertido();
                    if (cotizacion != null) cotizacion.CambiarEstado(EstadoCotizacion.Cerrada);
                    break;

                case ResultadoSeguimiento.Perdida:
                    if (lead != null) lead.MarcarComoNoInteresado();
                    if (cotizacion != null) cotizacion.CambiarEstado(EstadoCotizacion.Perdida);
                    break;
            }

            if (lead != null)
                lead.RegistrarActividad(DateTime.UtcNow);
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
                FechaContacto = s.FechaContacto,
                MedioContacto = s.MedioContacto.ToString(),
                Resultado = s.Resultado.ToString(),
                Notas = s.Notas,
                ProximoContacto = s.ProximoContacto,
                VendedorNombre = s.Vendedor?.NombreCompleto ?? "",
                LeadNombre = s.Lead?.NombreContacto,
                CotizacionNumero = s.Cotizacion?.NumeroCotizacion,
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

            // Alertas: asignados > 48h sin contactar
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

            // Alertas: contactados > 7 días sin cotizar
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