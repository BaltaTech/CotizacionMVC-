using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Models.Enums;

namespace CotizacionMVC.Data.Repositorios.Interfaces
{
    public interface ILeadRepository
    {
        
        Task<Lead?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<Lead>> GetAllAsync();
        Task AddAsync(Lead lead);
        void Update(Lead lead);
        void Delete(Lead lead);
        Task SaveChangesAsync();
        Task<IReadOnlyList<Lead>> ObtenerConClientesAsync();
        Task<IReadOnlyList<Lead>> ObtenerPorVendedorAsync(Guid vendedorId);
        Task<IReadOnlyList<Lead>> ObtenerPorClientesAsync(List<Guid> clienteIds); 
        Task<IReadOnlyList<Lead>> ObtenerSinVendedorPorClienteAsync(Guid clienteId); 
        Task<int> ContarPerdidosDesdeAsync(DateTime fechaInicio);
        Task<IReadOnlyList<Lead>> ObtenerTodosAsync();
        IQueryable<Lead> ObtenerQueryable();
    }
}