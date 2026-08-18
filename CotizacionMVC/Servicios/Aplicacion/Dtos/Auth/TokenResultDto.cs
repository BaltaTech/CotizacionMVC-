namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Auth
{
    public class TokenResultDto
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresInSeconds { get; set; }
    }
}