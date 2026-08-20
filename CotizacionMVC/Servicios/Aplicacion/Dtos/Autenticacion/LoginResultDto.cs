namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Autenticacion
{
    public class LoginResultDto
    {
        public bool Exitoso { get; set; }
        public string? MensajeError { get; set; }
        public string? NombreUsuario { get; set; }
        public string? Email { get; set; }
        public string? TokenJwt { get; set; } 

        public static LoginResultDto Exito(string nombreUsuario, string email, string? token = null)
        {
            return new LoginResultDto
            {
                Exitoso = true,
                NombreUsuario = nombreUsuario,
                Email = email,
                TokenJwt = token
            };
        }

        public static LoginResultDto Error(string mensaje)
        {
            return new LoginResultDto
            {
                Exitoso = false,
                MensajeError = mensaje
            };
        }
    }
}