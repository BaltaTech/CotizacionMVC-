using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Data.Repositorios.Implementaciones
{
    public class EmpresaRepository : BaseRepository<Empresa>, IEmpresaRepository
    {
        public EmpresaRepository(ApplicationDbContext context) : base(context) { }

        public async Task<Empresa?> ObtenerActivaAsync()
        {
            return await _context.Empresas
                .FirstOrDefaultAsync(e => e.Activa);
        }
    }
}
