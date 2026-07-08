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
    }
}
