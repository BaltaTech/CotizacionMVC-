using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Servicios.Aplicacion.Dtos.Cotizacion;
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

        public async Task<IEnumerable<CotizacionResumenDto>> ObtenerTodasConRelacionesAsync()
        {
            return await _context.Cotizaciones
                .AsNoTracking()
                .OrderByDescending(c => c.FechaCreacion)
                .Select(c => new CotizacionResumenDto
                {
                    Id = c.Id,
                    NumeroCotizacion = c.NumeroCotizacion,
                    ClienteNombre = c.Cliente.Nombre,
                    EmpresaNombre = c.Empresa.NombreComercial,
                    FechaCreacion = c.FechaCreacion,
                    EmpresaId = c.EmpresaId,
                    Total = c.Total.Monto,
                    Moneda = c.Empresa.MonedaBase,
                    Estado = c.Estado.ToString()
                })
                .ToListAsync();
        }

        public async Task<IEnumerable<CotizacionResumenDto>> ObtenerPorVendedorAsync(Guid vendedorId)
        {
            return await _context.Cotizaciones
                .AsNoTracking()
                .Where(c => c.VendedorId == vendedorId)
                .OrderByDescending(c => c.FechaCreacion)
                .Select(c => new CotizacionResumenDto
                {
                    Id = c.Id,
                    NumeroCotizacion = c.NumeroCotizacion,
                    ClienteNombre = c.Cliente.Nombre,
                    EmpresaNombre = c.Empresa.NombreComercial,
                    FechaCreacion = c.FechaCreacion,
                    EmpresaId = c.EmpresaId,
                    Total = c.Total.Monto,
                    Moneda = c.Empresa.MonedaBase,
                    Estado = c.Estado.ToString()
                })
                .ToListAsync();
        }

        public async Task<string> GenerarSiguienteNumeroAsync()
        {
            var ultimoNumero = await _context.Cotizaciones
                .AsNoTracking()
                .OrderByDescending(c => c.NumeroCotizacion)
                .Select(c => c.NumeroCotizacion)
                .FirstOrDefaultAsync();

            if (string.IsNullOrEmpty(ultimoNumero))
                return "COT-0001";

            var partes = ultimoNumero.Split('-');
            if (partes.Length == 2 && int.TryParse(partes[1], out int numero))
                return $"COT-{numero + 1:D4}";

            return "COT-0001";
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