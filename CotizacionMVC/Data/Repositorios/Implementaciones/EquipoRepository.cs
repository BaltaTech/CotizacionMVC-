using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Data.Repositorios.Implementaciones
{
    public class EquipoRepository : BaseRepository<Equipo>, IEquipoRepository
    {
        public EquipoRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Equipo>> ObtenerActivosOrdenadosAsync()
        {
            return await _context.Equipos
                .Where(e => e.Activo)
                .OrderBy(e => e.Marca)
                .ToListAsync();
        }
        public async Task<IEnumerable<Equipo>> ObtenerTodosOrdenadosAsync()
        {
            return await _context.Equipos
                .OrderBy(e => e.Marca)
                .ToListAsync();
        }
        public IQueryable<Equipo> ObtenerQueryable()
        {
            return _context.Equipos.AsQueryable();
        }
        public async Task<bool> EstaEnUsoAsync(Guid id)
        {
            return await _context.ItemsCotizacion.AnyAsync(i => i.EquipoId == id);
        }
    }
}