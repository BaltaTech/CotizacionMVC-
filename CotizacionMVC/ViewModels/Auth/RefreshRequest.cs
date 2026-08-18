using System.ComponentModel.DataAnnotations;

namespace CotizacionMVC.ViewModels.Auth
{
    public class RefreshRequest
    {
        [Required(ErrorMessage = "El refresh token es obligatorio")]
        public string RefreshToken { get; set; } = string.Empty;
    }
}