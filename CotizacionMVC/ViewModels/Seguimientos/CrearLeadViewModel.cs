using System.ComponentModel.DataAnnotations;

namespace CotizacionMVC.ViewModels.Seguimientos
{
    public class CrearLeadViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(200)]
        public string NombreContacto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;

        [MaxLength(200)]
        [EmailAddress(ErrorMessage = "Correo no válido")]
        public string? CorreoElectronico { get; set; }

        [MaxLength(300)]
        public string? EmpresaCliente { get; set; }

        [MaxLength(100)]
        public string? ProductoBusca { get; set; }

        [MaxLength(500)]
        public string? Comentarios { get; set; }
    }
}
