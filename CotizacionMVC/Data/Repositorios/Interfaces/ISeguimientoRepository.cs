using CotizacionMVC.Models.Entidades;

namespace CotizacionMVC.Data.Repositorios.Interfaces
{
    public interface ISeguimientoRepository
    {
        Task<Seguimiento?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<Seguimiento>> GetByLeadIdAsync(Guid leadId);
        Task<IReadOnlyList<Seguimiento>> GetByCotizacionIdAsync(Guid cotizacionId);
        Task<IReadOnlyList<Seguimiento>> GetByVendedorIdAsync(Guid vendedorId);
        Task<IReadOnlyList<Seguimiento>> GetPendientesHoyAsync(Guid vendedorId);
        Task<IReadOnlyList<Seguimiento>> GetVencidosAsync(Guid vendedorId);
        Task<int> GetCountByVendedorFechaAsync(Guid vendedorId, DateTime fecha);
        Task AddAsync(Seguimiento seguimiento);
        void Update(Seguimiento seguimiento);
        Task SaveChangesAsync();
    }
}
