using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using Microsoft.EntityFrameworkCore;

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
                .Include(c => c.Cliente)
                .Include(c => c.Empresa)
                .Include(c => c.Vendedor)
                .Include(c => c.ItemsEquipos)
                    .ThenInclude(i => i.Equipo)
                .Include(c => c.ItemsInstalacion)
                    .ThenInclude(i => i.Instalacion)
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<IEnumerable<Cotizacion>> ObtenerTodasConRelacionesAsync()
        {
            return await _context.Cotizaciones
                .Include(c => c.Cliente)
                .Include(c => c.Empresa)
                .Include(c => c.Vendedor)
                .OrderByDescending(c => c.FechaCreacion)
                .ToListAsync();
        }

        public async Task<IEnumerable<Cotizacion>> ObtenerPorVendedorAsync(Guid vendedorId)
        {
            return await _context.Cotizaciones
                .Include(c => c.Cliente)
                .Include(c => c.Empresa)
                .Include(c => c.Vendedor)
                .Where(c => c.VendedorId == vendedorId)
                .OrderByDescending(c => c.FechaCreacion)
                .ToListAsync();
        }

        public async Task<string> GenerarSiguienteNumeroAsync()
        {
            string prefijo = "COT";
            var ultimaCotizacion = await _context.Cotizaciones
                .OrderByDescending(c => c.NumeroCotizacion)
                .FirstOrDefaultAsync();

            int numero = 1;
            if (ultimaCotizacion != null)
            {
                var partes = ultimaCotizacion.NumeroCotizacion.Split('-');
                if (partes.Length == 2 && int.TryParse(partes[1], out int ultimoNumero))
                    numero = ultimoNumero + 1;
            }

            return $"{prefijo}-{numero:D4}";
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
    }
}
