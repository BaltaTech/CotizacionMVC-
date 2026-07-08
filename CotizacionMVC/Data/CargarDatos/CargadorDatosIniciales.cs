using CotizacionMVC.Models.Entidades;
using Microsoft.AspNetCore.Identity;

namespace CotizacionMVC.Data.CargaDatos
{
    public static class CargadorDatosIniciales
    {
        public static async Task CargarAsync(IServiceProvider proveedorServicios)
        {
            var gestorRoles = proveedorServicios.GetRequiredService<RoleManager<IdentityRole<Guid>>>();
            var gestorUsuarios = proveedorServicios.GetRequiredService<UserManager<Usuario>>();

            // Cargar roles si no existen
            string[] roles = { "Administrador", "Vendedor", "Recepcion" };
            foreach (var rol in roles)
            {
                if (!await gestorRoles.RoleExistsAsync(rol))
                    await gestorRoles.CreateAsync(new IdentityRole<Guid>(rol));
            }

            // Cargar usuario administrador por defecto
            var correoAdmin = "admin@empresa.com";
            var usuarioAdmin = await gestorUsuarios.FindByEmailAsync(correoAdmin);
            if (usuarioAdmin == null)
            {
                var admin = new Usuario("Administrador Principal", correoAdmin);
                var resultado = await gestorUsuarios.CreateAsync(admin, "Admin123!");
                if (resultado.Succeeded)
                {
                    await gestorUsuarios.AddToRoleAsync(admin, "Administrador");
                }
            }
        }
    }
}