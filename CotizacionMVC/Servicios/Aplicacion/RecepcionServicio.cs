using CotizacionMVC.Data;
using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Models.Enums;
using CotizacionMVC.Models.Valor;
using CotizacionMVC.Servicios.Aplicacion.Dtos.Recepcion;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using CotizacionMVC.Servicios.Infraestructura;
using CotizacionMVC.ViewModels.Recepcion;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Servicios.Aplicacion
{
    public class RecepcionServicio : IRecepcionServicio
    {
        private readonly IClienteRepository _clienteRepo;
        private readonly IEmpresaRepository _empresaRepo;
        private readonly UserManager<Usuario> _userManager;
        private readonly NotificacionServicio _notificacionServicio;
        private readonly ApplicationDbContext _context;
        private readonly IAutorizacionServicio _autorizacionServicio;

        public RecepcionServicio(
            IClienteRepository clienteRepo,
            IEmpresaRepository empresaRepo,
            UserManager<Usuario> userManager,
            NotificacionServicio notificacionServicio,
            ApplicationDbContext context,
            IAutorizacionServicio autorizacionServicio)
        {
            _clienteRepo = clienteRepo;
            _empresaRepo = empresaRepo;
            _userManager = userManager;
            _notificacionServicio = notificacionServicio;
            _context = context;
            _autorizacionServicio = autorizacionServicio;
        }

        public async Task<ResultadoRegistroCliente> RegistrarClienteAsync(
            RegistrarClienteViewModel modelo, Guid registradoPorId)
        {
            if (string.IsNullOrWhiteSpace(modelo.Nombre))
                return ResultadoRegistroCliente.Error("El nombre es obligatorio");
            if (string.IsNullOrWhiteSpace(modelo.Telefono))
                return ResultadoRegistroCliente.Error("El teléfono es obligatorio");
            if (string.IsNullOrWhiteSpace(modelo.CodigoPostal))
                return ResultadoRegistroCliente.Error("El código postal es obligatorio");
            if (string.IsNullOrWhiteSpace(modelo.ProductoBusca))
                return ResultadoRegistroCliente.Error("Debe seleccionar un producto");

            var clienteExistente = await _clienteRepo.ExisteTelefonoAsync(modelo.Telefono);
            var empresa = await _empresaRepo.GetByIdAsync(modelo.EmpresaId);
            if (empresa == null)
                return ResultadoRegistroCliente.Error("Empresa no encontrada");

            var esRecepcion = await _autorizacionServicio.EsRecepcionAsync(registradoPorId)
                              || await _autorizacionServicio.EsAdminAsync(registradoPorId);

            Cliente cliente;
            if (clienteExistente != null)
            {
                cliente = clienteExistente;
            }
            else
            {
                var contacto = new Contacto(modelo.Telefono, null, modelo.Correo, modelo.Nombre);
                cliente = new Cliente(modelo.Nombre, contacto);
                var folio = await _clienteRepo.GenerarFolioAsync();
                cliente.AsignarFolio(folio);

                if (!string.IsNullOrWhiteSpace(modelo.CodigoPostal) && modelo.CodigoPostal.Length == 5)
                {
                    var direccion = new Direccion(null, null, null, modelo.Ciudad, null, modelo.CodigoPostal, null);
                    cliente.ActualizarDireccion(direccion);
                }

                var observaciones = $"Producto: {modelo.ProductoBusca}";
                if (!string.IsNullOrWhiteSpace(modelo.Comentarios))
                    observaciones += $" | Comentarios: {modelo.Comentarios}";
                cliente.AgregarObservaciones(observaciones);
                cliente.ConfigurarRegistro(modelo.Origen, registradoPorId);
                await _clienteRepo.AddAsync(cliente);
            }

            var lead = new Lead(
                empresa,
                cliente.Nombre,
                modelo.Telefono,
                CategoriaLead.Caliente,
                modelo.Origen.ToString(),
                OrigenLead.Recepcion,
                modelo.Correo);

            lead.VincularCliente(cliente);
            lead.EstablecerProducto(modelo.ProductoBusca);
            if (!string.IsNullOrWhiteSpace(modelo.Comentarios))
                lead.AgregarComentario(modelo.Comentarios);

            if (esRecepcion && modelo.AsignarAhora && modelo.VendedorAsignadoId.HasValue)
            {
                var vendedor = await _userManager.FindByIdAsync(modelo.VendedorAsignadoId.Value.ToString());
                if (vendedor != null)
                {
                    lead.AsignarVendedor(vendedor);
                    cliente.AsignarVendedor(vendedor.Id);
                    await _notificacionServicio.EnviarNotificacionAsync(
                        modelo.VendedorAsignadoId.Value.ToString(),
                        "Nueva Oportunidad Asignada",
                        $"Se te ha asignado una oportunidad: {cliente.Nombre} - {modelo.ProductoBusca} - {empresa.NombreComercial}",
                        "success");
                }
            }
            else if (!esRecepcion)
            {
                var recepcionistas = await _userManager.GetUsersInRoleAsync("Recepcion");
                var admins = await _userManager.GetUsersInRoleAsync("Administrador");
                foreach (var recep in recepcionistas.Concat(admins))
                {
                    await _notificacionServicio.EnviarNotificacionAsync(
                        recep.Id.ToString(),
                        "Nuevo Cliente Pendiente",
                        $"El vendedor registró a {cliente.Nombre} - {modelo.ProductoBusca}. Requiere asignación.",
                        "warning");
                }
            }

            _context.Leads.Add(lead);
            await _context.SaveChangesAsync();
            return ResultadoRegistroCliente.Exito(cliente);
        }

        public async Task<List<Cliente>> BuscarPorTelefonoAsync(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono)) return new();
            var clientes = await _clienteRepo.GetAllAsync();
            return clientes.Where(c => c.Contacto.Telefono == telefono ||
                                       c.Contacto.TelefonoMovil == telefono).ToList();
        }

        public async Task<ResultadoRegistroCliente> AsignarVendedorAsync(Guid clienteId, Guid vendedorId)
        {
            var cliente = await _clienteRepo.GetByIdAsync(clienteId);
            if (cliente == null)
                return ResultadoRegistroCliente.Error("Cliente no encontrado");

            if (cliente.Estado != EstadoCliente.SinAsignar && cliente.VendedorAsignadoId.HasValue)
                return ResultadoRegistroCliente.Error("El cliente ya tiene vendedor asignado");

            var vendedor = await _userManager.FindByIdAsync(vendedorId.ToString());
            if (vendedor == null)
                return ResultadoRegistroCliente.Error("Vendedor no encontrado");

            cliente.AsignarVendedor(vendedorId);
            _clienteRepo.Update(cliente);

            var leadsSinAsignar = await _context.Leads
                .Where(l => l.ClienteId == clienteId && l.VendedorAsignadoId == null)
                .ToListAsync();

            foreach (var lead in leadsSinAsignar)
            {
                lead.AsignarVendedor(vendedor);
            }

            await _context.SaveChangesAsync();

            await _notificacionServicio.EnviarNotificacionAsync(
                vendedorId.ToString(), "Cliente Asignado",
                $"Se te ha asignado el cliente: {cliente.Nombre}", "success");

            return ResultadoRegistroCliente.Exito(cliente);
        }

        public async Task<ResultadoRegistroCliente> MarcarNoCotizableAsync(Guid clienteId, string motivo, string? comentario)
        {
            if (!Enum.TryParse<MotivoNoCotizable>(motivo, out var motivoEnum))
                return ResultadoRegistroCliente.Error("Motivo no válido");

            var cliente = await _clienteRepo.GetByIdAsync(clienteId);
            if (cliente == null) return ResultadoRegistroCliente.Error("Cliente no encontrado");

            cliente.MarcarNoCotizable(motivoEnum, comentario);
            _clienteRepo.Update(cliente);
            await _clienteRepo.SaveChangesAsync();
            return ResultadoRegistroCliente.Exito(cliente);
        }

        public async Task<IReadOnlyList<ClienteDashboardDto>> ObtenerDashboardAsync(Guid usuarioId)
        {
            var queryClientes = _context.Clientes.AsQueryable();

            // Solo filtrar clientes
            queryClientes = await _autorizacionServicio.FiltrarClientesAsync(usuarioId, queryClientes);

            var clientes = await queryClientes
                .OrderByDescending(c => c.FechaRegistro)
                .ToListAsync();

            // Obtener TODOS los leads de estos clientes en UNA sola consulta
            var clienteIds = clientes.Select(c => c.Id).ToList();
            var todosLosLeads = await _context.Leads
                .Where(l => l.ClienteId != null && clienteIds.Contains(l.ClienteId.Value))
                .OrderByDescending(l => l.FechaCreacion)
                .ToListAsync();

            return clientes.Select(c =>
            {
                // Buscar el lead más reciente para este cliente
                var ultimoLead = todosLosLeads
                    .Where(l => l.ClienteId == c.Id)
                    .OrderByDescending(l => l.FechaCreacion)
                    .FirstOrDefault();

                // DETERMINAR EL ESTADO CORRECTO
                string estado;

                if (c.Estado == EstadoCliente.Cotizado || c.Estado == EstadoCliente.NoCotizable || c.Estado == EstadoCliente.Cerrado)
                {
                    estado = c.Estado.ToString();
                }
                else if (ultimoLead != null)
                {
                    estado = ultimoLead.Estado.ToString();
                }
                else
                {
                    estado = c.Estado.ToString();
                }

                return new ClienteDashboardDto
                {
                    Id = c.Id,
                    Folio = c.Folio,
                    Nombre = c.Nombre,
                    Telefono = c.Contacto.Telefono,
                    Origen = c.Origen.ToString(),
                    Estado = estado,
                    FechaRegistro = c.FechaRegistro,
                    Observaciones = c.Observaciones,
                    VendedorAsignadoId = c.VendedorAsignadoId ?? ultimoLead?.VendedorAsignadoId
                };
            }).ToList();
        }

        public async Task<List<UltimoRegistroDto>> ObtenerUltimosRegistrosAsync(Guid usuarioId)
        {
            var queryClientes = _context.Clientes.AsQueryable();

            // Solo filtrar clientes
            queryClientes = await _autorizacionServicio.FiltrarClientesAsync(usuarioId, queryClientes);

            var clientes = await queryClientes
                .OrderByDescending(c => c.FechaRegistro)
                .Take(10)
                .ToListAsync();

            // Obtener leads de estos clientes
            var clienteIds = clientes.Select(c => c.Id).ToList();
            var todosLosLeads = await _context.Leads
                .Where(l => l.ClienteId != null && clienteIds.Contains(l.ClienteId.Value))
                .OrderByDescending(l => l.FechaCreacion)
                .ToListAsync();

            return clientes.Select(c =>
            {
                var ultimoLead = todosLosLeads
                    .Where(l => l.ClienteId == c.Id)
                    .FirstOrDefault();

                string estado;
                if (c.Estado == EstadoCliente.Cotizado || c.Estado == EstadoCliente.NoCotizable || c.Estado == EstadoCliente.Cerrado)
                {
                    estado = c.Estado.ToString();
                }
                else if (ultimoLead != null)
                {
                    estado = ultimoLead.Estado.ToString();
                }
                else
                {
                    estado = c.Estado.ToString();
                }

                return new UltimoRegistroDto
                {
                    Id = c.Id,
                    Folio = c.Folio,
                    Nombre = c.Nombre,
                    Telefono = c.Contacto.Telefono,
                    Origen = c.Origen.ToString(),
                    Fecha = c.FechaRegistro,
                    Estado = estado,
                    VendedorAsignadoId = c.VendedorAsignadoId ?? ultimoLead?.VendedorAsignadoId,
                    Observaciones = c.Observaciones
                };
            }).ToList();
        }

        // ⚠️ ELIMINÉ EL SEGUNDO MÉTODO DUPLICADO QUE USABA c.Leads

        public async Task<ClienteDetalleRecepcionDto?> ObtenerDetalleClienteAsync(Guid id)
        {
            var cliente = await _clienteRepo.GetByIdAsync(id);
            if (cliente == null) return null;

            string? vendedorNombre = null;
            if (cliente.VendedorAsignadoId.HasValue)
            {
                var vendedor = await _userManager.FindByIdAsync(cliente.VendedorAsignadoId.Value.ToString());
                vendedorNombre = vendedor?.NombreCompleto;
            }

            var producto = "No especificado";
            if (!string.IsNullOrWhiteSpace(cliente.Observaciones))
            {
                var partes = cliente.Observaciones.Split("Producto: ");
                if (partes.Length > 1) producto = partes[1].Split(" |")[0];
            }

            return new ClienteDetalleRecepcionDto
            {
                Folio = cliente.Folio,
                Nombre = cliente.Nombre,
                Telefono = cliente.Contacto.Telefono,
                TelefonoMovil = cliente.Contacto.TelefonoMovil,
                Correo = cliente.Contacto.Correo,
                Origen = cliente.Origen.ToString(),
                Estado = cliente.Estado.ToString(),
                Producto = producto,
                VendedorNombre = vendedorNombre,
                FechaRegistro = cliente.FechaRegistro,
                Observaciones = cliente.Observaciones
            };
        }

        public async Task<List<VendedorResumenDto>> ObtenerVendedoresActivosAsync()
        {
            var vendedores = await _userManager.GetUsersInRoleAsync("Vendedor");
            return vendedores.Where(v => v.Activo)
                .Select(v => new VendedorResumenDto { Id = v.Id, NombreCompleto = v.NombreCompleto, Email = v.Email })
                .OrderBy(v => v.NombreCompleto).ToList();
        }

        public async Task<List<EmpresaResumenDto>> ObtenerEmpresasAsync()
        {
            var empresas = await _empresaRepo.GetAllAsync();
            return empresas.Select(e => new EmpresaResumenDto { Id = e.Id, NombreComercial = e.NombreComercial }).ToList();
        }
    }
}