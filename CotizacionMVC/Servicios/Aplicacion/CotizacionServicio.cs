using CotizacionMVC.Data;
using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Models.Enums;
using CotizacionMVC.Servicios.Aplicacion.Dtos.Cotizacion;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Servicios.Aplicacion
{
    public class CotizacionServicio : ICotizacionServicio
    {
        private readonly ICotizacionRepository _cotizacionRepo;
        private readonly IClienteRepository _clienteRepo;
        private readonly IEquipoRepository _equipoRepo;
        private readonly IInstalacionRepository _instalacionRepo;
        private readonly IEmpresaRepository _empresaRepo;
        private readonly IDocumento _documentoService;
        private readonly ApplicationDbContext _context;

        public CotizacionServicio(
            ICotizacionRepository cotizacionRepo,
            IClienteRepository clienteRepo,
            IEquipoRepository equipoRepo,
            IInstalacionRepository instalacionRepo,
            IEmpresaRepository empresaRepo,
            IDocumento documentoService,
            ApplicationDbContext context)
        {
            _cotizacionRepo = cotizacionRepo;
            _clienteRepo = clienteRepo;
            _equipoRepo = equipoRepo;
            _instalacionRepo = instalacionRepo;
            _empresaRepo = empresaRepo;
            _documentoService = documentoService;
            _context = context;
        }

        // ==================== QUERIES ====================

        public async Task<IReadOnlyList<CotizacionResumenDto>> ObtenerIndiceAsync(Guid? vendedorId, Guid? empresaId, bool esAdmin)
        {
            IEnumerable<CotizacionResumenDto> cotizaciones;

            if (esAdmin)
                cotizaciones = await _cotizacionRepo.ObtenerTodasConRelacionesAsync();
            else if (vendedorId.HasValue)
                cotizaciones = await _cotizacionRepo.ObtenerPorVendedorAsync(vendedorId.Value);
            else
                cotizaciones = new List<CotizacionResumenDto>();

            if (empresaId.HasValue)
                cotizaciones = cotizaciones.Where(c => c.EmpresaId == empresaId.Value);

            return cotizaciones
                .OrderByDescending(c => c.FechaCreacion)
                .ToList();
        }

        public async Task<CotizacionDetalleDto?> ObtenerDetalleAsync(Guid id)
        {
            var cotizacion = await _cotizacionRepo.ObtenerCompletaPorIdAsync(id);

            if (cotizacion == null)
                return null;

            return MapearADetalleDto(cotizacion);
        }

        public async Task<IReadOnlyList<LeadResumenDto>> ObtenerLeadsDelVendedorAsync(Guid vendedorId)
        {
            return await _context.Leads
                .Include(l => l.Cliente)
                .Include(l => l.Empresa)
                .Where(l => l.VendedorAsignadoId == vendedorId)
                .OrderByDescending(l => l.FechaAsignacion)
                .Select(l => new LeadResumenDto
                {
                    Id = l.Id,
                    ClienteNombre = l.Cliente != null ? l.Cliente.Nombre : l.NombreContacto ?? "",
                    ClienteId = l.ClienteId,
                    Telefono = l.Telefono,
                    ProductoBusca = l.ProductoBusca,
                    EmpresaNombre = l.Empresa != null ? l.Empresa.NombreComercial : "",
                    Estado = (int)l.Estado,
                    FechaAsignacion = l.FechaAsignacion,
                    FechaCreacion = l.FechaCreacion,
                    NombreContacto = l.NombreContacto,
                    ClienteTelefono = l.Cliente != null ? l.Cliente.Contacto.Telefono : l.Telefono,
                    EmpresaId = l.EmpresaId,
                    EmpresaColorPrimario = l.Empresa != null ? l.Empresa.ColorPrimario : null,
                    EmpresaEslogan = l.Empresa != null ? l.Empresa.Eslogan : null,
                    OrigenLead = (int)l.OrigenLead
                })
                .ToListAsync();
        }

        public async Task<DatosCrearCotizacionDto> ObtenerDatosParaCrearAsync(Guid usuarioId, bool esVendedor, Guid? leadId)
        {
            var datos = new DatosCrearCotizacionDto();

            // Lead específico
            if (leadId.HasValue)
            {
                var lead = await _context.Leads
                    .Include(l => l.Empresa)
                    .Include(l => l.Cliente)
                    .FirstOrDefaultAsync(l => l.Id == leadId.Value);

                if (lead != null && lead.VendedorAsignadoId == usuarioId)
                {
                    datos.Lead = new LeadResumenDto
                    {
                        Id = lead.Id,
                        ClienteNombre = lead.Cliente?.Nombre ?? "",
                        ClienteId = lead.ClienteId,
                        Telefono = lead.Telefono,
                        ProductoBusca = lead.ProductoBusca,
                        EmpresaNombre = lead.Empresa?.NombreComercial ?? "",
                        EmpresaId = lead.EmpresaId,
                        EmpresaColorPrimario = lead.Empresa?.ColorPrimario,
                        EmpresaEslogan = lead.Empresa?.Eslogan,
                        Estado = (int)lead.Estado,
                        FechaAsignacion = lead.FechaAsignacion,
                        FechaCreacion = lead.FechaCreacion,
                        ClienteTelefono = lead.Cliente?.Contacto?.Telefono ?? lead.Telefono
                    };
                }
            }

            // Clientes
            if (esVendedor)
            {
                var leadsDelVendedor = await _context.Leads
                    .Include(l => l.Cliente)
                    .Where(l => l.VendedorAsignadoId == usuarioId)
                    .OrderByDescending(l => l.FechaAsignacion)
                    .ToListAsync();

                datos.Clientes = leadsDelVendedor
                    .Where(l => l.Cliente != null)
                    .Select(l => new ClienteResumenDto
                    {
                        Id = l.Cliente!.Id,
                        Nombre = l.Cliente.Nombre,
                        Telefono = l.Cliente.Contacto.Telefono,
                        Folio = l.Cliente.Folio,
                        Estado = (int)l.Cliente.Estado,
                        FechaRegistro = l.Cliente.FechaRegistro,
                        Observaciones = l.Cliente.Observaciones,
                        TieneVendedor = l.Cliente.VendedorAsignadoId.HasValue
                    })
                    .Distinct()
                    .ToList();
            }
            else
            {
                var clientes = await _clienteRepo.ObtenerParaCotizacionAsync();
                datos.Clientes = clientes.Select(c => new ClienteResumenDto
                {
                    Id = c.Id,
                    Nombre = c.Nombre,
                    Telefono = c.Contacto.Telefono,
                    Folio = c.Folio,
                    Estado = (int)c.Estado,
                    FechaRegistro = c.FechaRegistro,
                    Observaciones = c.Observaciones,
                    TieneVendedor = c.VendedorAsignadoId.HasValue
                }).ToList();
            }

            // Equipos
            var equipos = await _equipoRepo.ObtenerTodosOrdenadosAsync();
            datos.Equipos = equipos.Select(e => new EquipoResumenDto
            {
                Id = e.Id,
                Marca = e.Marca.ToString(),
                Modelo = e.Modelo,
                CapacidadToneladas = e.CapacidadToneladas,
                PrecioBase = e.PrecioBase,
                MonedaOriginal = e.MonedaOriginal
            }).ToList();

            // Instalaciones
            var instalaciones = await _instalacionRepo.ObtenerActivasAsync();
            datos.Instalaciones = instalaciones.Select(i => new InstalacionResumenDto
            {
                Id = i.Id,
                Concepto = i.Concepto,
                Descripcion = i.Descripcion,
                CostoUnitario = i.CostoUnitario
            }).ToList();

            return datos;
        }

        // ==================== COMANDOS ====================
        public async Task<ResultadoCotizacionDto> CrearAsync(CrearCotizacionDto dto)
        {
            if (dto.ClienteId == Guid.Empty)
                return ResultadoCotizacionDto.Error("Debe seleccionar un cliente");

            if (dto.Equipos == null || !dto.Equipos.Any())
                return ResultadoCotizacionDto.Error("Debe agregar al menos un equipo");

            if (dto.AreaMetrosCuadrados <= 0)
                return ResultadoCotizacionDto.Error("El área debe ser mayor a cero");

            var cliente = await _clienteRepo.GetByIdAsync(dto.ClienteId);
            if (cliente == null)
                return ResultadoCotizacionDto.Error("Cliente no encontrado");

            var empresa = await _empresaRepo.GetByIdAsync(dto.EmpresaId);
            if (empresa == null)
                return ResultadoCotizacionDto.Error("Empresa no encontrada");

            if (!cliente.TieneContacto())
                return ResultadoCotizacionDto.Error("El cliente no tiene información de contacto.");

            if (!cliente.TieneDireccion())
                return ResultadoCotizacionDto.Error("El cliente no tiene dirección registrada.");

            foreach (var eq in dto.Equipos)
            {
                var equipo = await _equipoRepo.GetByIdAsync(eq.EquipoId);
                if (equipo == null)
                    return ResultadoCotizacionDto.Error($"Equipo con ID {eq.EquipoId} no encontrado");

                if (empresa.EsExclusivaTrane && !equipo.EsMarcaTrane())
                    return ResultadoCotizacionDto.Error(
                        $"Esta empresa solo puede cotizar equipos Trane. El equipo {equipo.Marca} {equipo.Modelo} no está permitido.");

                if (!equipo.Activo)
                    return ResultadoCotizacionDto.Error($"El equipo {equipo.Modelo} no está disponible actualmente.");
            }

            var numeroCotizacion = await _cotizacionRepo.GenerarSiguienteNumeroAsync();
            var vendedor = await _context.Users.FindAsync(dto.VendedorId);

            if (vendedor == null)
                return ResultadoCotizacionDto.Error("Vendedor no encontrado");

            Cotizacion cotizacion;
            try
            {
                cotizacion = new Cotizacion(numeroCotizacion, cliente, empresa, vendedor,
     dto.AreaMetrosCuadrados, dto.CondicionesPago,
     dto.TipoCambio, dto.RecargoCiudadPorcentaje);
            }
            catch (ArgumentException ex)
            {
                return ResultadoCotizacionDto.Error(ex.Message);
            }

            foreach (var eq in dto.Equipos)
            {
                var equipo = await _equipoRepo.GetByIdAsync(eq.EquipoId);
                cotizacion.AgregarEquipo(equipo!, eq.Cantidad, empresa.UtilidadEmpresaPorcentaje,
                    empresa.UtilidadVendedorPorcentaje, null);
            }

            foreach (var inst in dto.Instalaciones)
            {
                if (inst.InstalacionId.HasValue)
                {
                    var instalacion = await _instalacionRepo.GetByIdAsync(inst.InstalacionId.Value);
                    if (instalacion == null || !instalacion.Activo) continue;
                    cotizacion.AgregarInstalacionPredefinida(instalacion, inst.Cantidad);
                }
                else
                {
                    cotizacion.AgregarInstalacion(inst.Concepto, inst.Descripcion ?? "", inst.Cantidad, inst.CostoUnitario);
                }
            }

            await _cotizacionRepo.AddAsync(cotizacion);
            await _cotizacionRepo.SaveChangesAsync();

            if (dto.LeadId.HasValue)
            {
                var lead = await _context.Leads.FindAsync(dto.LeadId.Value);
                if (lead != null)
                {
                    cotizacion.VincularLead(lead);
                    lead.MarcarCotizado();
                    lead.ActualizarCategoria(CategoriaLead.Cotizando);
                    await _context.SaveChangesAsync();
                }
            }

            var detalle = MapearADetalleDto(cotizacion);
            return ResultadoCotizacionDto.Exito(detalle);
        }

        public async Task<ResultadoCotizacionDto> ActualizarAsync(ActualizarCotizacionDto dto)
        {
            var cotizacion = await _cotizacionRepo.ObtenerConItemsAsync(dto.Id);

            if (cotizacion == null)
                return ResultadoCotizacionDto.Error("Cotización no encontrada");

            if (!cotizacion.PuedeSerModificada())
                return ResultadoCotizacionDto.Error("Esta cotización no puede ser modificada");

            var cliente = await _clienteRepo.GetByIdAsync(dto.ClienteId);
            if (cliente == null)
                return ResultadoCotizacionDto.Error("Cliente no encontrado");

            try
            {
                cotizacion.ActualizarDatosBasicos(cliente, dto.AreaMetrosCuadrados, dto.CondicionesPago);
            }
            catch (Exception ex) when (ex is ArgumentException || ex is InvalidOperationException)
            {
                return ResultadoCotizacionDto.Error(ex.Message);
            }

            _cotizacionRepo.Update(cotizacion);
            await _cotizacionRepo.SaveChangesAsync();

            var detalle = MapearADetalleDto(cotizacion);
            return ResultadoCotizacionDto.Exito(detalle);
        }

        public async Task<ResultadoCotizacionDto> EliminarAsync(Guid id)
        {
            var cotizacion = await _cotizacionRepo.ObtenerConItemsAsync(id);

            if (cotizacion == null)
                return ResultadoCotizacionDto.Error("Cotización no encontrada");

            if (!cotizacion.PuedeSerModificada())
                return ResultadoCotizacionDto.Error("No se puede eliminar una cotización en este estado");

            _cotizacionRepo.Delete(cotizacion);
            await _cotizacionRepo.SaveChangesAsync();

            return ResultadoCotizacionDto.Exito(null!);
        }

        public async Task<ResultadoCotizacionDto> CambiarEstadoAsync(Guid id, int nuevoEstado)
        {
            var cotizacion = await _cotizacionRepo.GetByIdAsync(id);

            if (cotizacion == null)
                return ResultadoCotizacionDto.Error("Cotización no encontrada");

            try
            {
                cotizacion.CambiarEstado((EstadoCotizacion)nuevoEstado);
            }
            catch (Exception ex)
            {
                return ResultadoCotizacionDto.Error(ex.Message);
            }

            _cotizacionRepo.Update(cotizacion);
            await _cotizacionRepo.SaveChangesAsync();

            var detalle = MapearADetalleDto(cotizacion);
            return ResultadoCotizacionDto.Exito(detalle);
        }

        public async Task<byte[]> GenerarPdfAsync(Guid id)
        {
            var cotizacion = await _cotizacionRepo.ObtenerCompletaPorIdAsync(id);

            if (cotizacion == null)
                throw new KeyNotFoundException("Cotización no encontrada");

            var pdfBytes = _documentoService.Generar(cotizacion);

            var carpeta = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "pdf", "cotizaciones");
            if (!Directory.Exists(carpeta))
                Directory.CreateDirectory(carpeta);

            var nombreArchivo = $"{cotizacion.NumeroCotizacion}{_documentoService.ExtensionArchivo}";
            var rutaCompleta = Path.Combine(carpeta, nombreArchivo);
            await File.WriteAllBytesAsync(rutaCompleta, pdfBytes);

            var rutaRelativa = $"wwwroot/pdf/cotizaciones/{nombreArchivo}";
            cotizacion.GuardarRutaPdf(rutaRelativa);
            _cotizacionRepo.Update(cotizacion);
            await _cotizacionRepo.SaveChangesAsync();

            return pdfBytes;
        }

        public Task<decimal> CalcularCargaTermicaAsync(decimal area)
        {
            var trSugerida = area / 16;
            return Task.FromResult(Math.Round(trSugerida, 1));
        }

        // ==================== MÉTODOS PRIVADOS ====================

        private CotizacionDetalleDto MapearADetalleDto(Cotizacion cotizacion)
        {
            return new CotizacionDetalleDto
            {
                Id = cotizacion.Id,
                NumeroCotizacion = cotizacion.NumeroCotizacion,
                ClienteId = cotizacion.ClienteId,
                ClienteNombre = cotizacion.Cliente?.Nombre ?? "",
                EmpresaId = cotizacion.EmpresaId,
                EmpresaNombre = cotizacion.Empresa?.NombreComercial ?? "",
                VendedorId = cotizacion.VendedorId,
                VendedorNombre = cotizacion.Vendedor?.NombreCompleto ?? "",
                FechaCreacion = cotizacion.FechaCreacion,
                FechaVencimiento = cotizacion.FechaVencimiento,
                Estado = cotizacion.Estado,
                AreaMetrosCuadrados = cotizacion.AreaMetrosCuadrados,
                CondicionesPago = cotizacion.CondicionesPago,
                Subtotal = cotizacion.Subtotal?.Monto ?? 0,
                Iva = cotizacion.Iva?.Monto ?? 0,
                Total = cotizacion.Total?.Monto ?? 0,
                Moneda = cotizacion.Empresa?.MonedaBase ?? "MXN",
                PuedeSerModificada = cotizacion.PuedeSerModificada(),
                Equipos = cotizacion.ItemsEquipos.Select(i => new ItemCotizacionDto
                {
                    EquipoMarca = i.Equipo?.Marca.ToString() ?? "",
                    EquipoModelo = i.Equipo?.Modelo ?? "",
                    Cantidad = i.Cantidad,
                    PrecioUnitario = i.PrecioUnitario?.Monto ?? 0,
                    Subtotal = i.Subtotal?.Monto ?? 0
                }).ToList(),
                Instalaciones = cotizacion.ItemsInstalacion.Select(i => new ItemInstalacionDto
                {
                    Concepto = i.Concepto,
                    Descripcion = i.Descripcion ?? "",
                    Cantidad = i.Cantidad,
                    CostoUnitario = i.CostoUnitario?.Monto ?? 0,
                    Subtotal = i.Subtotal?.Monto ?? 0
                }).ToList()
            };
        }
    }
}