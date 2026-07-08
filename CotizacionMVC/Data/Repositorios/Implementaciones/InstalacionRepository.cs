using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Data.Repositorios.Implementaciones
{
    public class InstalacionRepository : BaseRepository<Instalacion>, IInstalacionRepository
    {
        public InstalacionRepository(ApplicationDbContext context) : base(context) { }

        public async Task<IEnumerable<Instalacion>> ObtenerActivasAsync()
        {
            return await _context.Instalaciones
                .Where(i => i.Activo)
                .ToListAsync();
        }
    }
}
