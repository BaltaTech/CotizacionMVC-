using CotizacionMVC.Data;
using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Models.Enums;
using CotizacionMVC.Models.Valor;
using CotizacionMVC.Servicios.Infraestructura;
using CotizacionMVC.ViewModels.Recepcion;
using Microsoft.AspNetCore.Identity;

namespace CotizacionMVC.Servicios.Aplicacion
{
    public class RecepcionServicio
    {
        private readonly IClienteRepository _clienteRepo;
        private readonly IEmpresaRepository _empresaRepo;
        private readonly UserManager<Usuario> _userManager;
        private readonly NotificacionServicio _notificacionServicio;
        private readonly ApplicationDbContext _context;

        public RecepcionServicio(
            IClienteRepository clienteRepo,
            IEmpresaRepository empresaRepo,
            UserManager<Usuario> userManager,
            NotificacionServicio notificacionServicio,
            ApplicationDbContext context)
        {
            _clienteRepo = clienteRepo;
            _empresaRepo = empresaRepo;
            _userManager = userManager;
            _notificacionServicio = notificacionServicio;
            _context = context;
        }

        public async Task<ResultadoRegistroCliente> RegistrarClienteAsync(
            RegistrarClienteViewModel modelo,
            Guid registradoPorId,
            bool esRecepcion)
        {
            // 1. Validar datos de entrada
            if (string.IsNullOrWhiteSpace(modelo.Nombre))
                return ResultadoRegistroCliente.Error("El nombre es obligatorio");

            if (string.IsNullOrWhiteSpace(modelo.Telefono))
                return ResultadoRegistroCliente.Error("El teléfono es obligatorio");

            if (string.IsNullOrWhiteSpace(modelo.CodigoPostal))
                return ResultadoRegistroCliente.Error("El código postal es obligatorio");

            if (string.IsNullOrWhiteSpace(modelo.ProductoBusca))
                return ResultadoRegistroCliente.Error("Debe seleccionar un producto");

            // 2. Verificar duplicado por teléfono
            var clienteExistente = await _clienteRepo.ExisteTelefonoAsync(modelo.Telefono);

            // 3. Verificar que la empresa existe
            var empresa = await _empresaRepo.GetByIdAsync(modelo.EmpresaId);
            if (empresa == null)
                return ResultadoRegistroCliente.Error("Empresa no encontrada");

            Cliente cliente;

            if (clienteExistente != null)
            {
                // El cliente YA existe → solo creamos un nuevo Lead
                cliente = clienteExistente;
            }
            else
            {
                // Cliente NUEVO → creamos Cliente + Lead
                var contacto = new Contacto(modelo.Telefono, null, modelo.Correo, modelo.Nombre);
                cliente = new Cliente(modelo.Nombre, contacto);

                var folio = await _clienteRepo.GenerarFolioAsync();
                cliente.AsignarFolio(folio);

                var direccion = new Direccion(null, null, null, modelo.Ciudad, null, modelo.CodigoPostal, null);
                cliente.ActualizarDireccion(direccion);

                var observaciones = $"Producto: {modelo.ProductoBusca}";
                if (!string.IsNullOrWhiteSpace(modelo.Comentarios))
                    observaciones += $" | Comentarios: {modelo.Comentarios}";
                cliente.AgregarObservaciones(observaciones);

                cliente.ConfigurarRegistro(modelo.Origen, registradoPorId);

                await _clienteRepo.AddAsync(cliente);
            }

            // ========== CREAR LEAD (OPORTUNIDAD) ==========
            var lead = new Lead(
                empresa,
                cliente.Nombre,
                modelo.Telefono,
                CategoriaLead.Caliente, // Lead entrante = caliente
                modelo.Origen.ToString(),
                modelo.Correo
            );

            lead.VincularCliente(cliente);
            lead.EstablecerProducto(modelo.ProductoBusca);

            if (!string.IsNullOrWhiteSpace(modelo.Comentarios))
                lead.AgregarComentario(modelo.Comentarios);

            // Asignar vendedor al Lead
            if (esRecepcion && modelo.AsignarAhora && modelo.VendedorAsignadoId.HasValue)
            {
                var vendedor = await _userManager.FindByIdAsync(modelo.VendedorAsignadoId.Value.ToString());
                if (vendedor != null)
                {
                    lead.AsignarVendedor(vendedor);

                    // Notificar al vendedor
                    await _notificacionServicio.EnviarNotificacionAsync(
                        modelo.VendedorAsignadoId.Value.ToString(),
                        "Nueva Oportunidad Asignada",
                        $"Se te ha asignado una oportunidad: {cliente.Nombre} - {modelo.ProductoBusca} - {empresa.NombreComercial}",
                        "success"
                    );
                }
            }

            _context.Leads.Add(lead);
            await _context.SaveChangesAsync();

            return ResultadoRegistroCliente.Exito(cliente);
        }

        public async Task<List<Cliente>> BuscarPorTelefonoAsync(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                return new List<Cliente>();

            var clientes = await _clienteRepo.GetAllAsync();
            return clientes.Where(c => c.Contacto.Telefono == telefono ||
                                       c.Contacto.TelefonoMovil == telefono)
                           .ToList();
        }

        public async Task<ResultadoRegistroCliente> AsignarVendedorAsync(
            Guid clienteId, Guid vendedorId)
        {
            var cliente = await _clienteRepo.GetByIdAsync(clienteId);
            if (cliente == null)
                return ResultadoRegistroCliente.Error("Cliente no encontrado");

            if (cliente.Estado != EstadoCliente.SinAsignar)
                return ResultadoRegistroCliente.Error("El cliente ya tiene vendedor asignado");

            cliente.AsignarVendedor(vendedorId);
            _clienteRepo.Update(cliente);

            await _notificacionServicio.EnviarNotificacionAsync(
                vendedorId.ToString(),
                "Cliente Asignado",
                $"Se te ha asignado el cliente: {cliente.Nombre}",
                "success"
            );

            return ResultadoRegistroCliente.Exito(cliente);
        }

        public async Task<ResultadoRegistroCliente> ReasignarVendedorAsync(
            Guid clienteId, Guid nuevoVendedorId)
        {
            var cliente = await _clienteRepo.GetByIdAsync(clienteId);
            if (cliente == null)
                return ResultadoRegistroCliente.Error("Cliente no encontrado");

            cliente.ReasignarVendedor(nuevoVendedorId);
            _clienteRepo.Update(cliente);

            await _notificacionServicio.EnviarNotificacionAsync(
                nuevoVendedorId.ToString(),
                "Cliente Reasignado",
                $"Se te ha reasignado el cliente: {cliente.Nombre}",
                "warning"
            );

            return ResultadoRegistroCliente.Exito(cliente);
        }

        public async Task<ResultadoRegistroCliente> MarcarNoCotizableAsync(
            Guid clienteId, MotivoNoCotizable motivo, string? comentario = null)
        {
            var cliente = await _clienteRepo.GetByIdAsync(clienteId);
            if (cliente == null)
                return ResultadoRegistroCliente.Error("Cliente no encontrado");

            cliente.MarcarNoCotizable(motivo, comentario);
            _clienteRepo.Update(cliente);

            return ResultadoRegistroCliente.Exito(cliente);
        }
    }
}