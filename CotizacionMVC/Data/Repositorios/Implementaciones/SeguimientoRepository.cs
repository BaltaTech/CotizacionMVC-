using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Data.Repositorios.Implementaciones
{
    public class SeguimientoRepository : ISeguimientoRepository
    {
        private readonly ApplicationDbContext _context;

        public SeguimientoRepository(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<Seguimiento?> GetByIdAsync(Guid id)
        {
            return await _context.Seguimientos
                .Include(s => s.Lead)
                .Include(s => s.Cotizacion)
                .Include(s => s.Vendedor)
                .FirstOrDefaultAsync(s => s.Id == id);
        }

        public async Task<IReadOnlyList<Seguimiento>> GetByLeadIdAsync(Guid leadId)
        {
            return await _context.Seguimientos
                .Include(s => s.Vendedor)
                .Where(s => s.LeadId == leadId)
                .OrderByDescending(s => s.FechaContacto)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Seguimiento>> GetByCotizacionIdAsync(Guid cotizacionId)
        {
            return await _context.Seguimientos
                .Include(s => s.Vendedor)
                .Where(s => s.CotizacionId == cotizacionId)
                .OrderByDescending(s => s.FechaContacto)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Seguimiento>> GetByVendedorIdAsync(Guid vendedorId)
        {
            return await _context.Seguimientos
                .Include(s => s.Lead)
                .Include(s => s.Cotizacion)
                .Where(s => s.VendedorId == vendedorId)
                .OrderByDescending(s => s.FechaContacto)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Seguimiento>> GetPendientesHoyAsync(Guid vendedorId)
        {
            var hoy = DateTime.UtcNow.Date;
            return await _context.Seguimientos
                .Include(s => s.Lead)
                .Include(s => s.Cotizacion)
                .Where(s => s.VendedorId == vendedorId
                    && s.ProximoContacto.HasValue
                    && s.ProximoContacto.Value.Date == hoy)
                .OrderBy(s => s.ProximoContacto)
                .ToListAsync();
        }

        public async Task<IReadOnlyList<Seguimiento>> GetVencidosAsync(Guid vendedorId)
        {
            var hoy = DateTime.UtcNow.Date;
            return await _context.Seguimientos
                .Include(s => s.Lead)
                .Include(s => s.Cotizacion)
                .Where(s => s.VendedorId == vendedorId
                    && s.ProximoContacto.HasValue
                    && s.ProximoContacto.Value.Date < hoy)
                .OrderBy(s => s.ProximoContacto)
                .ToListAsync();
        }

        public async Task<int> GetCountByVendedorFechaAsync(Guid vendedorId, DateTime fecha)
        {
            return await _context.Seguimientos
                .Where(s => s.VendedorId == vendedorId
                    && s.FechaContacto.Date == fecha.Date)
                .CountAsync();
        }

        public async Task AddAsync(Seguimiento seguimiento)
        {
            await _context.Seguimientos.AddAsync(seguimiento);
        }

        public void Update(Seguimiento seguimiento)
        {
            _context.Seguimientos.Update(seguimiento);
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}