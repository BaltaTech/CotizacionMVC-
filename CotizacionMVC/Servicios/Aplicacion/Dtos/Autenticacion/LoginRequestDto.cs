namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Autenticacion
{
    public class LoginRequestDto
    {
        public string Email { get; set; } = "";
        public string Password { get; set; } = "";
        public bool Recordarme { get; set; }
    }
}