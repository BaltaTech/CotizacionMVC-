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

        public RecepcionServicio(
            IClienteRepository clienteRepo,
            IEmpresaRepository empresaRepo,
            UserManager<Usuario> userManager,
            NotificacionServicio notificacionServicio)
        {
            _clienteRepo = clienteRepo;
            _empresaRepo = empresaRepo;
            _userManager = userManager;
            _notificacionServicio = notificacionServicio;
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
            if (clienteExistente != null)
            {
                return ResultadoRegistroCliente.Duplicado(
                    clienteExistente,
                    $"El teléfono {modelo.Telefono} ya está registrado a nombre de {clienteExistente.Nombre}. " +
                    $"Folio: {clienteExistente.Folio}");
            }

            // 3. Verificar que la empresa existe
            var empresa = await _empresaRepo.GetByIdAsync(modelo.EmpresaId);
            if (empresa == null)
                return ResultadoRegistroCliente.Error("Empresa no encontrada");

            // 4. Crear el contacto
            var contacto = new Contacto(
                modelo.Telefono,
                null,
                modelo.Correo,
                modelo.Nombre
            );

            // 5. Crear el cliente
            var cliente = new Cliente(modelo.Nombre, contacto);

            // 6. Generar y asignar folio
            var folio = await _clienteRepo.GenerarFolioAsync();
            cliente.AsignarFolio(folio);

            // 7. Guardar ubicación con CP
            var direccion = new Direccion(
                null, null, null,
                modelo.Ciudad,
                null,
                modelo.CodigoPostal,
                null
            );
            cliente.ActualizarDireccion(direccion);

            // 8. Agregar observaciones
            var observaciones = $"Producto: {modelo.ProductoBusca}";
            if (!string.IsNullOrWhiteSpace(modelo.Comentarios))
                observaciones += $" | Comentarios: {modelo.Comentarios}";
            cliente.AgregarObservaciones(observaciones);

            // 9. Configurar registro
            cliente.ConfigurarRegistro(modelo.Origen, registradoPorId);

            // 10. Asignar vendedor Y NOTIFICAR
            if (esRecepcion && modelo.AsignarAhora && modelo.VendedorAsignadoId.HasValue)
            {
                cliente.AsignarVendedor(modelo.VendedorAsignadoId.Value);

                // Enviar notificación al vendedor
                await _notificacionServicio.EnviarNotificacionAsync(
                    modelo.VendedorAsignadoId.Value.ToString(),
                    "Nuevo Cliente Asignado",
                    $"Se te ha asignado el cliente: {cliente.Nombre} - {modelo.ProductoBusca}",
                    "success"
                );
            }
            else
            {
                cliente.MarcarPendienteAsignar();
            }

            // 11. Guardar en base de datos
            await _clienteRepo.AddAsync(cliente);

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

            // Notificar al vendedor
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

            // Notificar al nuevo vendedor
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