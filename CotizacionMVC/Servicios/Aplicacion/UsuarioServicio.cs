using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Servicios.Aplicacion.Dtos.Usuarios;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using CotizacionMVC.ViewModels.Usuarios;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Servicios.Aplicacion
{
    public class UsuarioServicio : IUsuarioServicio
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly RoleManager<IdentityRole<Guid>> _roleManager;
        private readonly IEmpresaRepository _empresaRepository;

        public UsuarioServicio(
            UserManager<Usuario> userManager,
            RoleManager<IdentityRole<Guid>> roleManager,
            IEmpresaRepository empresaRepository)
        {
            _userManager = userManager;
            _roleManager = roleManager;
            _empresaRepository = empresaRepository;
        }

        public async Task<IReadOnlyList<UsuarioConRolesDto>> ObtenerTodosAsync()
        {
            var usuarios = await _userManager.Users
                .OrderBy(u => u.NombreCompleto)
                .ToListAsync();

            var resultado = new List<UsuarioConRolesDto>();

            foreach (var usuario in usuarios)
            {
                var roles = await _userManager.GetRolesAsync(usuario);
                resultado.Add(new UsuarioConRolesDto
                {
                    Id = usuario.Id,
                    NombreCompleto = usuario.NombreCompleto,
                    Email = usuario.Email!,
                    Activo = usuario.Activo,
                    FechaRegistro = usuario.FechaRegistro,
                    UltimoAcceso = usuario.UltimoAcceso,
                    Roles = roles.ToList()
                });
            }

            return resultado;
        }

        public async Task<List<string>> ObtenerRolesAsync()
        {
            return await _roleManager.Roles.Select(r => r.Name).ToListAsync();
        }
        
        public async Task<List<Empresa>> ObtenerEmpresasAsync()
        {
            return (await _empresaRepository.GetAllAsync()).ToList();
        }

        public async Task<ResultadoCrearUsuario> CrearAsync(CrearUsuarioViewModel modelo)
        {
            if (string.IsNullOrWhiteSpace(modelo.NombreCompleto))
                return ResultadoCrearUsuario.Error("El nombre completo es obligatorio");

            if (string.IsNullOrWhiteSpace(modelo.Email))
                return ResultadoCrearUsuario.Error("El correo electrónico es obligatorio");

            if (string.IsNullOrWhiteSpace(modelo.Password) || modelo.Password.Length < 6)
                return ResultadoCrearUsuario.Error("La contraseña debe tener al menos 6 caracteres");

            if (modelo.EmpresaId == Guid.Empty)
                return ResultadoCrearUsuario.Error("Debe seleccionar una empresa");

            var existe = await _userManager.FindByEmailAsync(modelo.Email);
            if (existe != null)
                return ResultadoCrearUsuario.Error("El correo electrónico ya está registrado");

            var usuario = new Usuario(modelo.NombreCompleto, modelo.Email);
            var resultado = await _userManager.CreateAsync(usuario, modelo.Password);

            if (!resultado.Succeeded)
            {
                var errores = string.Join(", ", resultado.Errors.Select(e => e.Description));
                return ResultadoCrearUsuario.Error($"Error al crear usuario: {errores}");
            }

            if (!string.IsNullOrEmpty(modelo.Rol))
            {
                var rolExiste = await _roleManager.RoleExistsAsync(modelo.Rol);
                if (rolExiste)
                    await _userManager.AddToRoleAsync(usuario, modelo.Rol);
            }

            var empresa = await _empresaRepository.GetByIdAsync(modelo.EmpresaId);
            if (empresa != null)
            {
                empresa.AgregarUsuarioAcceso(usuario);
                await _empresaRepository.SaveChangesAsync();
            }

            return ResultadoCrearUsuario.Exito(usuario.Id, usuario.NombreCompleto, usuario.Email);
        }

        public async Task<ResultadoOperacion> CambiarEstadoAsync(Guid usuarioId)
        {
            var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
            if (usuario == null)
                return ResultadoOperacion.Error("Usuario no encontrado");

            if (usuario.Activo)
                usuario.Desactivar();
            else
                usuario.Activar();

            await _userManager.UpdateAsync(usuario);

            return ResultadoOperacion.Exito();
        }

        public async Task<ResultadoOperacion> CambiarPasswordAsync(Guid usuarioId, string nuevaPassword)
        {
            if (string.IsNullOrWhiteSpace(nuevaPassword) || nuevaPassword.Length < 6)
                return ResultadoOperacion.Error("La contraseña debe tener al menos 6 caracteres");

            var usuario = await _userManager.FindByIdAsync(usuarioId.ToString());
            if (usuario == null)
                return ResultadoOperacion.Error("Usuario no encontrado");

            var token = await _userManager.GeneratePasswordResetTokenAsync(usuario);
            var resultado = await _userManager.ResetPasswordAsync(usuario, token, nuevaPassword);

            if (!resultado.Succeeded)
            {
                var errores = string.Join(", ", resultado.Errors.Select(e => e.Description));
                return ResultadoOperacion.Error($"Error al cambiar contraseña: {errores}");
            }

            return ResultadoOperacion.Exito();
        }
    }
}