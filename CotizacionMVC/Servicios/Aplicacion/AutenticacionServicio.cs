using CotizacionMVC.Models.Entidades;
using CotizacionMVC.Servicios.Aplicacion.Dtos.Autenticacion;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using CotizacionMVC.ViewModels;
using Microsoft.AspNetCore.Identity;

namespace CotizacionMVC.Servicios.Aplicacion
{
    public class AutenticacionServicio : IAutenticacionServicio
    {
        private readonly UserManager<Usuario> _userManager;
        private readonly SignInManager<Usuario> _signInManager;

        public AutenticacionServicio(
            UserManager<Usuario> userManager,
            SignInManager<Usuario> signInManager)
        {
            _userManager = userManager;
            _signInManager = signInManager;
        }

        public async Task<LoginResultDto> LoginConCookiesAsync(LoginViewModel modelo)
        {
            var usuario = await _userManager.FindByEmailAsync(modelo.Email);
            if (usuario == null || !usuario.Activo)
                return LoginResultDto.Error("Correo o contraseña incorrectos.");

            var resultado = await _signInManager.PasswordSignInAsync(
                usuario, modelo.Password, modelo.Recordarme, lockoutOnFailure: false);

            if (!resultado.Succeeded)
                return LoginResultDto.Error("Correo o contraseña incorrectos.");

            usuario.RegistrarAcceso();
            await _userManager.UpdateAsync(usuario);

            return LoginResultDto.Exito(usuario.NombreCompleto, usuario.Email);
        }

        public async Task<LoginResultDto> LoginAsync(LoginRequestDto request)
        {
            
            var usuario = await _userManager.FindByEmailAsync(request.Email);
            if (usuario == null || !usuario.Activo)
                return LoginResultDto.Error("Correo o contraseña incorrectos.");

            var resultado = await _signInManager.PasswordSignInAsync(
                usuario, request.Password, request.Recordarme, lockoutOnFailure: false);

            if (!resultado.Succeeded)
                return LoginResultDto.Error("Correo o contraseña incorrectos.");

            return LoginResultDto.Exito(usuario.NombreCompleto, usuario.Email);
        }

        public async Task LogoutAsync()
        {
            await _signInManager.SignOutAsync();
        }
    }
}