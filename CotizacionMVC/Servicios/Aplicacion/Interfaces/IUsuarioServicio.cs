using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Servicios.Aplicacion.Dtos.Usuarios;
using CotizacionMVC.ViewModels.Usuarios;

namespace CotizacionMVC.Servicios.Aplicacion.Interfaces
{
    public interface IUsuarioServicio
    {
        Task<IReadOnlyList<UsuarioConRolesDto>> ObtenerTodosAsync();
        Task<ResultadoCrearUsuario> CrearAsync(CrearUsuarioViewModel modelo);
        Task<ResultadoOperacion> CambiarEstadoAsync(Guid usuarioId);
        Task<ResultadoOperacion> CambiarPasswordAsync(Guid usuarioId, string nuevaPassword);
        Task<List<string>> ObtenerRolesAsync();
        Task<List<Empresa>> ObtenerEmpresasAsync();
    }
}