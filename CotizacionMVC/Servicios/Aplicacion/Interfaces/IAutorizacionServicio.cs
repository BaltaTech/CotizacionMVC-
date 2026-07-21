using CotizacionMVC.Models.Entidades;

namespace CotizacionMVC.Servicios.Aplicacion.Interfaces
{
    public interface IAutorizacionServicio
    {
        Task<bool> EsAdminAsync(Guid usuarioId);
        Task<bool> EsVendedorAsync(Guid usuarioId);
        Task<bool> EsRecepcionAsync(Guid usuarioId);
        Task<List<string>> ObtenerRolesAsync(Guid usuarioId);
        Task<bool> PuedeAccederASeccionAsync(Guid usuarioId, string seccion);
        Task<bool> TieneAccesoAEmpresaAsync(Guid usuarioId, Guid empresaId);
        Task<Guid?> ObtenerEmpresaActivaIdAsync(Guid usuarioId);
        Task<Empresa?> ObtenerEmpresaActivaAsync(Guid usuarioId);
        Task<IQueryable<Cotizacion>> FiltrarCotizacionesAsync(Guid usuarioId, IQueryable<Cotizacion> query);
        Task<IQueryable<Lead>> FiltrarLeadsAsync(Guid usuarioId, IQueryable<Lead> query);
        Task<IQueryable<Cliente>> FiltrarClientesAsync(Guid usuarioId, IQueryable<Cliente> query);
    }
}