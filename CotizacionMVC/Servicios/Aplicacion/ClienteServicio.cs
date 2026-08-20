using CotizacionMVC.Data;
using CotizacionMVC.Data.Repositorios.Implementaciones;
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
        private readonly ISeguimientoRepository _seguimientoRepository; // 

        
        public ClienteServicio(
            IClienteRepository clienteRepository,
            IAutorizacionServicio autorizacionServicio,
            ISeguimientoRepository seguimientoRepository) 
        {
            _clienteRepository = clienteRepository;
            _autorizacionServicio = autorizacionServicio;
            _seguimientoRepository = seguimientoRepository; 
        }

        public async Task<IReadOnlyList<ClienteResumenDto>> ObtenerTodosAsync(Guid usuarioId, string? termino = null)
        {
            // ========== 1. CONSTRUIR LA CONSULTA BASE ==========
            var query = _clienteRepository.ObtenerQueryable();
            query = await _autorizacionServicio.FiltrarClientesAsync(usuarioId, query);

            // ========== 2. INCLUIR TODAS LAS RELACIONES NECESARIAS ==========
            // ✅ Una sola consulta con todas las relaciones
            query = query
                .Include(c => c.Cotizaciones)
                    .ThenInclude(co => co.Empresa)
                .Include(c => c.Cotizaciones)
                    .ThenInclude(co => co.Seguimientos)
                .Include(c => c.Cotizaciones)
                    .ThenInclude(co => co.ItemsEquipos);

            // ========== 3. APLICAR FILTRO DE BÚSQUEDA ==========
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

            // ========== 4. PAGINACIÓN (OBLIGATORIA) ==========
            const int pageSize = 50;
            var page = 1; // O recibir como parámetro

            var clientes = await query
                .OrderBy(c => c.Nombre)
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .Select(c => new
                {
                    c.Id,
                    c.Nombre,
                    Telefono = c.Contacto.Telefono,
                    Correo = c.Contacto.Correo,
                    Estado = c.Estado.ToString(),
                    c.FechaRegistro,

                    // ✅ Ahora las cotizaciones ya están cargadas en memoria
                    Cotizaciones = c.Cotizaciones
                        .OrderByDescending(co => co.FechaCreacion)
                        .ToList(),

                    // ✅ Última cotización (pre-calculada)
                    UltimaCotizacion = c.Cotizaciones
                        .OrderByDescending(co => co.FechaCreacion)
                        .FirstOrDefault(),

                    // ✅ Seguimientos (pre-cargados)
                    Seguimientos = c.Cotizaciones
                        .SelectMany(co => co.Seguimientos)
                        .Where(s => s.ProximoContacto.HasValue)
                        .ToList()
                })
                .ToListAsync();

            // ========== 5. PROCESAR EN MEMORIA ==========
            var hoy = DateTime.UtcNow.Date;

            return clientes.Select(c =>
            {
                // Ya tenemos los datos en memoria, sin nuevas consultas
                var ultimaCotizacion = c.UltimaCotizacion;
                var cantidadCotizaciones = c.Cotizaciones.Count;
                var empresa = c.Cotizaciones
                    .OrderByDescending(co => co.FechaCreacion)
                    .Select(co => co.Empresa?.NombreComercial)
                    .FirstOrDefault() ?? "Sin empresa";

                var moneda = c.Cotizaciones
                    .OrderByDescending(co => co.FechaCreacion)
                    .Select(co => co.Empresa?.MonedaBase)
                    .FirstOrDefault() ?? "MXN";

                // ✅ Seguimientos: próximo contacto más cercano
                var proximosContactos = c.Seguimientos
                    .Where(s => s.ProximoContacto.HasValue)
                    .OrderBy(s => s.ProximoContacto)
                    .ToList();

                var proximoContacto = proximosContactos.FirstOrDefault()?.ProximoContacto;
                var tieneSeguimientoHoy = proximosContactos.Any(s =>
                    s.ProximoContacto.HasValue &&
                    s.ProximoContacto.Value.Date == hoy);

                // ✅ EsCaliente: verificar estado de cotizaciones
                var esCaliente = c.Cotizaciones.Any(co =>
                    co.Estado == EstadoCotizacion.InformacionSolicitada ||
                    co.Estado == EstadoCotizacion.CotizacionEnviada);

                // ✅ Última fecha de actividad
                var ultimaFechaActividad = c.Cotizaciones
                    .OrderByDescending(co => co.FechaCreacion)
                    .FirstOrDefault()?.FechaCreacion;

                var diasSinActividad = ultimaFechaActividad.HasValue
                    ? (hoy - ultimaFechaActividad.Value).Days
                    : (hoy - c.FechaRegistro).Days;

                return new ClienteResumenDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Telefono = c.Telefono ?? "Sin teléfono",
                    Correo = c.Correo ?? "Sin correo",
                    Estado = c.Estado,
                    CantidadCotizaciones = cantidadCotizaciones,
                    Empresa = empresa,
                    FechaRegistro = c.FechaRegistro,
                    UltimaFechaSeguimiento = ultimaFechaActividad,
                    ProximaFechaSeguimiento = proximoContacto,
                    DiasSinActividad = diasSinActividad,
                    TotalUltimaCotizacion = ultimaCotizacion?.Total?.Monto ?? 0,
                    Moneda = moneda,
                    TieneSeguimientoHoy = tieneSeguimientoHoy,
                    EsCaliente = esCaliente
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

            cliente.AsignarFolio(Cliente.GenerarFolio());

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

        // ==================== MÉTODOS PRIVADOS ====================

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

        private async Task<Dictionary<Guid, (DateTime? ProximoContacto, bool EsHoy)>>
            ObtenerInfoSeguimientosAsync(List<Guid> clienteIds)
        {
            if (!clienteIds.Any())
                return new Dictionary<Guid, (DateTime?, bool)>();

            var hoy = DateTime.UtcNow.Date;

            var seguimientos = await _seguimientoRepository
                .ObtenerPorClientesAsync(clienteIds);

            return seguimientos
                .Where(s => (s.Lead?.ClienteId ?? s.Cotizacion?.ClienteId) != null)
                .GroupBy(s => (s.Lead?.ClienteId ?? s.Cotizacion?.ClienteId)!.Value)
                .ToDictionary(
                    g => g.Key,
                    g => (
                        ProximoContacto: (DateTime?)g.Max(s => s.ProximoContacto),
                        EsHoy: g.Any(s => s.ProximoContacto.HasValue && s.ProximoContacto.Value.Date == hoy)
                    )
                );
        }
    }
}