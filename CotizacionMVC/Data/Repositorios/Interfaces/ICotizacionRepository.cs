using CotizacionMVC.Models.Entidades;
using Microsoft.EntityFrameworkCore.Storage;
using System.Data;

namespace CotizacionMVC.Data.Repositorios.Interfaces
{
    public interface ICotizacionRepository : IRepository<Cotizacion>
    {
        Task<Cotizacion?> ObtenerCompletaPorIdAsync(Guid id);
        Task<Cotizacion?> ObtenerConItemsAsync(Guid id);
        Task<Cotizacion?> ObtenerConClienteAsync(Guid id);
        IQueryable<Cotizacion> ObtenerQueryable();
        Task<string?> ObtenerUltimoNumeroAsync();
        Task<IDbContextTransaction> BeginTransactionAsync(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted);
    }
}