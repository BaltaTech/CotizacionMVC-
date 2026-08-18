using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data; 

namespace CotizacionMVC.Data.Repositorios.Implementaciones
{
    public class CotizacionRepository : BaseRepository<Cotizacion>, ICotizacionRepository
    {
        public CotizacionRepository(ApplicationDbContext context) : base(context)
        {
        }

        public async Task<Cotizacion?> ObtenerCompletaPorIdAsync(Guid id)
        {
            return await _context.Cotizaciones
                .AsNoTracking()
                .Include(c => c.Cliente)
                .Include(c => c.Empresa)
                .Include(c => c.Vendedor)
                .Include(c => c.ItemsEquipos)
                    .ThenInclude(i => i.Equipo)
                .Include(c => c.ItemsInstalacion)
                    .ThenInclude(i => i.Instalacion)
                .AsSplitQuery()
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Cotizacion?> ObtenerConItemsAsync(Guid id)
        {
            return await _context.Cotizaciones
                .Include(c => c.ItemsEquipos)
                .Include(c => c.ItemsInstalacion)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Cotizacion?> ObtenerConClienteAsync(Guid id)
        {
            return await _context.Cotizaciones
                .Include(c => c.Cliente)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public IQueryable<Cotizacion> ObtenerQueryable()
        {
            return _context.Cotizaciones.AsQueryable();
        }

        public async Task<string?> ObtenerUltimoNumeroAsync()
        {
            return await _context.Cotizaciones
                .AsNoTracking()
                .OrderByDescending(c => c.NumeroCotizacion)
                .Select(c => c.NumeroCotizacion)
                .FirstOrDefaultAsync();
        }

        public async Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
        {
            return await _context.Database.BeginTransactionAsync(isolationLevel);
        }
    }
}