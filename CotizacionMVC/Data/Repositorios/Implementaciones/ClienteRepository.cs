using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Data.Repositorios.Implementaciones
{
    public class ClienteRepository : BaseRepository<Cliente>, IClienteRepository
    {
        public ClienteRepository(ApplicationDbContext context) : base(context) { }

        public async Task<string> GenerarFolioAsync()
        {
            var fecha = DateTime.UtcNow.ToString("yyMMdd");
            var guidCorto = Guid.NewGuid().ToString("N")[..6].ToUpper();
            return $"CLI-{fecha}-{guidCorto}";
        }

        public async Task<Cliente?> ExisteTelefonoAsync(string telefono)
        {
            if (string.IsNullOrWhiteSpace(telefono))
                return null;

            return await _context.Clientes
                .FirstOrDefaultAsync(c => c.Contacto.Telefono == telefono ||
                                          c.Contacto.TelefonoMovil == telefono);
        }

        public async Task<IEnumerable<Cliente>> ObtenerTodosOrdenadosAsync()
        {
            return await _context.Clientes
                .OrderBy(c => c.Nombre)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cliente>> ObtenerParaCotizacionAsync()
        {
            return await _context.Clientes
                .Where(c => c.Estado == Models.Enums.EstadoCliente.Asignado ||
                            c.Estado == Models.Enums.EstadoCliente.Contactado ||
                            c.Estado == Models.Enums.EstadoCliente.SinAsignar)
                .OrderByDescending(c => c.FechaRegistro)
                .ToListAsync();
        }

        public IQueryable<Cliente> ObtenerQueryable()
        {
            return _context.Clientes.AsQueryable();
        }

        public async Task<Dictionary<Guid, (DateTime? ProximoContacto, bool EsHoy)>> ObtenerInfoSeguimientosAsync(List<Guid> clienteIds)
        {
            if (!clienteIds.Any())
                return new Dictionary<Guid, (DateTime?, bool)>();

            var hoy = DateTime.UtcNow.Date;

            var seguimientos = await _context.Seguimientos
                .Include(s => s.Lead)
                .Include(s => s.Cotizacion)
                .Where(s => s.ProximoContacto.HasValue)
                .Where(s => (s.Lead != null && s.Lead.ClienteId != null && clienteIds.Contains(s.Lead.ClienteId.Value)) ||
                            (s.Cotizacion != null && clienteIds.Contains(s.Cotizacion.ClienteId)))
                .ToListAsync();

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