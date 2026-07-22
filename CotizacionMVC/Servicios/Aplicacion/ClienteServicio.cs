using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Models.Enums;
using CotizacionMVC.Models.Valor;
using CotizacionMVC.Servicios.Aplicacion.Dtos.Cliente;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Servicios.Aplicacion
{
    public class ClienteServicio : IClienteServicio
    {
        private readonly IClienteRepository _clienteRepository;
        private readonly IAutorizacionServicio _autorizacionServicio;

        public ClienteServicio(
            IClienteRepository clienteRepository,
            IAutorizacionServicio autorizacionServicio)
        {
            _clienteRepository = clienteRepository;
            _autorizacionServicio = autorizacionServicio;
        }

        public async Task<IReadOnlyList<ClienteResumenDto>> ObtenerTodosAsync(Guid usuarioId, string? termino = null)
        {
            var query = _clienteRepository.ObtenerQueryable();
            query = await _autorizacionServicio.FiltrarClientesAsync(usuarioId, query);
            query = query.Include(c => c.Cotizaciones).ThenInclude(co => co.Empresa);

            if (!string.IsNullOrWhiteSpace(termino))
            {
                termino = termino.ToLower();
                query = query.Where(c =>
                    c.Nombre.ToLower().Contains(termino) ||
                    (c.Contacto.Telefono != null && c.Contacto.Telefono.ToLower().Contains(termino)) ||
                    (c.Contacto.TelefonoMovil != null && c.Contacto.TelefonoMovil.ToLower().Contains(termino)) ||
                    (c.Contacto.Correo != null && c.Contacto.Correo.ToLower().Contains(termino))
                );
            }

            var hoy = DateTime.UtcNow.Date;

            var clientes = await query
                .OrderBy(c => c.Nombre)
                .Select(c => new
                {
                    c.Id,
                    c.Nombre,
                    Telefono = c.Contacto.Telefono,
                    Correo = c.Contacto.Correo,
                    Estado = c.Estado.ToString(),
                    CantidadCotizaciones = c.Cotizaciones.Count,
                    Empresa = c.Cotizaciones.OrderByDescending(co => co.FechaCreacion)
                        .Select(co => co.Empresa.NombreComercial).FirstOrDefault(),
                    c.FechaRegistro,
                    UltimaFechaCotizacion = c.Cotizaciones.Any()
                        ? c.Cotizaciones.OrderByDescending(co => co.FechaCreacion)
                            .Select(co => (DateTime?)co.FechaCreacion).FirstOrDefault()
                        : null,
                    TotalUltimaCotizacion = c.Cotizaciones.Any()
                        ? c.Cotizaciones.OrderByDescending(co => co.FechaCreacion)
                            .Select(co => co.Total.Monto).FirstOrDefault()
                        : 0,
                    Moneda = c.Cotizaciones.Any()
                        ? c.Cotizaciones.OrderByDescending(co => co.FechaCreacion)
                            .Select(co => co.Empresa.MonedaBase).FirstOrDefault() ?? "MXN"
                        : "MXN",
                    EsCaliente = c.Cotizaciones.Any(co =>
                        co.Estado == EstadoCotizacion.InformacionSolicitada ||
                        co.Estado == EstadoCotizacion.CotizacionEnviada)
                })
                .ToListAsync();

            var clienteIds = clientes.Select(c => c.Id).ToList();
            var infoSeguimientos = await _clienteRepository.ObtenerInfoSeguimientosAsync(clienteIds);

            return clientes.Select(c =>
            {
                infoSeguimientos.TryGetValue(c.Id, out var info);
                return new ClienteResumenDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Telefono = c.Telefono,
                    Correo = c.Correo,
                    Estado = c.Estado,
                    CantidadCotizaciones = c.CantidadCotizaciones,
                    Empresa = c.Empresa,
                    FechaRegistro = c.FechaRegistro,
                    UltimaFechaSeguimiento = c.UltimaFechaCotizacion,
                    ProximaFechaSeguimiento = info.ProximoContacto,
                    DiasSinActividad = c.UltimaFechaCotizacion.HasValue
                        ? (hoy - c.UltimaFechaCotizacion.Value).Days
                        : (hoy - c.FechaRegistro).Days,
                    TotalUltimaCotizacion = c.TotalUltimaCotizacion,
                    Moneda = c.Moneda,
                    TieneSeguimientoHoy = info.EsHoy,
                    EsCaliente = c.EsCaliente
                };
            }).ToList();
        }

        public async Task<ClienteDetalleDto?> ObtenerPorIdAsync(Guid id)
        {
            var query = _clienteRepository.ObtenerQueryable()
                .Include(c => c.Cotizaciones)
                .ThenInclude(cot => cot.Empresa);

            var cliente = await query.FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
                return null;

            return MapearADetalleDto(cliente);
        }

        public async Task<ClienteDetalleDto?> ObtenerParaEdicionAsync(Guid id)
        {
            var query = _clienteRepository.ObtenerQueryable()
                .Include(c => c.Cotizaciones);

            var cliente = await query.FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
                return null;

            return MapearADetalleDto(cliente);
        }

        public async Task<ClienteDetalleDto?> ObtenerParaEliminacionAsync(Guid id)
        {
            var query = _clienteRepository.ObtenerQueryable()
                .Include(c => c.Cotizaciones);

            var cliente = await query.FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
                return null;

            return MapearADetalleDto(cliente);
        }

        public async Task<ClienteDetalleDto> CrearAsync(CrearClienteDto dto)
        {
            ValidarAlMenosUnMedioDeContacto(dto.Telefono, dto.TelefonoMovil, dto.Correo);

            var contacto = new Contacto(dto.Telefono, dto.TelefonoMovil, dto.Correo, dto.NombreContacto);
            var cliente = new Cliente(dto.Nombre, contacto);

            var direccion = ConstruirDireccion(dto.Calle, dto.NumeroExterior, dto.NumeroInterior,
                dto.Colonia, dto.Ciudad, dto.Estado, dto.CodigoPostal);

            if (direccion != null)
                cliente.ActualizarDireccion(direccion);

            if (!string.IsNullOrWhiteSpace(dto.Observaciones))
                cliente.AgregarObservaciones(dto.Observaciones);

            await _clienteRepository.AddAsync(cliente);
            await _clienteRepository.SaveChangesAsync();

            return MapearADetalleDto(cliente);
        }

        public async Task<ClienteDetalleDto> ActualizarAsync(ActualizarClienteDto dto)
        {
            ValidarAlMenosUnMedioDeContacto(dto.Telefono, dto.TelefonoMovil, dto.Correo);

            var cliente = await _clienteRepository.GetByIdAsync(dto.Id)
                ?? throw new KeyNotFoundException($"No se encontró el cliente con ID {dto.Id}");

            var nuevoContacto = new Contacto(dto.Telefono, dto.TelefonoMovil, dto.Correo, dto.NombreContacto);
            cliente.ActualizarContacto(nuevoContacto);

            var direccion = ConstruirDireccion(dto.Calle, dto.NumeroExterior, dto.NumeroInterior,
                dto.Colonia, dto.Ciudad, dto.Estado, dto.CodigoPostal);

            cliente.ActualizarDireccion(direccion);
            cliente.AgregarObservaciones(dto.Observaciones);

            _clienteRepository.Update(cliente);
            await _clienteRepository.SaveChangesAsync();

            return MapearADetalleDto(cliente);
        }

        public async Task<EliminarClienteResultado> EliminarAsync(Guid id)
        {
            var query = _clienteRepository.ObtenerQueryable()
                .Include(c => c.Cotizaciones);

            var cliente = await query.FirstOrDefaultAsync(c => c.Id == id);

            if (cliente == null)
                throw new KeyNotFoundException($"No se encontró el cliente con ID {id}");

            if (cliente.Cotizaciones.Any())
            {
                return new EliminarClienteResultado
                {
                    Exitoso = false,
                    MotivoFallo = $"No se puede eliminar el cliente {cliente.Nombre} porque tiene cotizaciones asociadas"
                };
            }

            _clienteRepository.Delete(cliente);
            await _clienteRepository.SaveChangesAsync();

            return new EliminarClienteResultado { Exitoso = true };
        }

        private void ValidarAlMenosUnMedioDeContacto(string? telefono, string? telefonoMovil, string? correo)
        {
            bool tieneTelefono = !string.IsNullOrWhiteSpace(telefono);
            bool tieneMovil = !string.IsNullOrWhiteSpace(telefonoMovil);
            bool tieneCorreo = !string.IsNullOrWhiteSpace(correo);

            if (!tieneTelefono && !tieneMovil && !tieneCorreo)
                throw new ArgumentException("El cliente debe tener al menos un medio de contacto (teléfono, teléfono móvil o correo electrónico)");
        }

        private Direccion? ConstruirDireccion(string? calle, string? numeroExterior, string? numeroInterior,
            string? colonia, string? ciudad, string? estado, string? codigoPostal)
        {
            bool tieneDireccion = !string.IsNullOrWhiteSpace(calle) ||
                                  !string.IsNullOrWhiteSpace(colonia) ||
                                  !string.IsNullOrWhiteSpace(ciudad) ||
                                  !string.IsNullOrWhiteSpace(codigoPostal);

            if (!tieneDireccion)
                return null;

            return new Direccion(
                calle ?? "",
                numeroExterior,
                colonia ?? "",
                ciudad ?? "",
                estado,
                codigoPostal ?? "",
                numeroInterior
            );
        }

        private ClienteDetalleDto MapearADetalleDto(Cliente cliente)
        {
            return new ClienteDetalleDto
            {
                Id = cliente.Id,
                Nombre = cliente.Nombre,
                Telefono = cliente.Contacto.Telefono,
                TelefonoMovil = cliente.Contacto.TelefonoMovil,
                Correo = cliente.Contacto.Correo,
                NombreContacto = cliente.Contacto.NombreContacto,
                Calle = cliente.Direccion?.Calle,
                NumeroExterior = cliente.Direccion?.NumeroExterior,
                NumeroInterior = cliente.Direccion?.NumeroInterior,
                Colonia = cliente.Direccion?.Colonia,
                Ciudad = cliente.Direccion?.Ciudad,
                Estado = cliente.Direccion?.Estado,
                CodigoPostal = cliente.Direccion?.CodigoPostal,
                Observaciones = cliente.Observaciones,
                EstadoCliente = cliente.Estado,
                FechaCreacion = cliente.FechaRegistro,
                Cotizaciones = cliente.Cotizaciones.Select(c => new CotizacionResumenDto
                {
                    NumeroCotizacion = c.NumeroCotizacion,
                    EmpresaNombre = c.Empresa?.NombreComercial,
                    FechaCreacion = c.FechaCreacion,
                    Total = c.Total.ToString(),
                    Estado = c.Estado.ToString()
                }).ToList()
            };
        }
    }
}