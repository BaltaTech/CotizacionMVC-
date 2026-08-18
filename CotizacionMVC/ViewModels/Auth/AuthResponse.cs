namespace CotizacionMVC.ViewModels.Auth
{
    public class AuthResponse
    {
        public string Token { get; set; } = string.Empty;
        public string RefreshToken { get; set; } = string.Empty;
        public int ExpiresIn { get; set; }
        public string? Message { get; set; }
        public bool Success { get; set; }

        public static AuthResponse CreateSuccess(string token, string refreshToken, int expiresInSeconds)
        {
            return new AuthResponse
            {
                Success = true,
                Token = token,
                RefreshToken = refreshToken,
                ExpiresIn = expiresInSeconds,
                Message = "Autenticación exitosa"
            };
        }

        public static AuthResponse CreateError(string message)
        {
            return new AuthResponse
            {
                Success = false,
                Message = message
            };
        }
    }
}