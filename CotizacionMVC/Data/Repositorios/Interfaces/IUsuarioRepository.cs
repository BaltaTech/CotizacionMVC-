using CotizacionMVC.Models.Entidades;

namespace CotizacionMVC.Data.Repositorios.Interfaces
{
    public interface IUsuarioRepository
    {
        Task<Usuario?> GetByIdAsync(Guid id);
        Task<IReadOnlyList<Usuario>> ObtenerActivosAsync();
        Task<IReadOnlyList<Usuario>> ObtenerPorRolAsync(string rol);
        Task AddAsync(Usuario usuario);
        void Update(Usuario usuario);
        Task SaveChangesAsync();
    }
}
