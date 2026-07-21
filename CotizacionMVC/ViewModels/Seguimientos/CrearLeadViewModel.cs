using System.ComponentModel.DataAnnotations;

namespace CotizacionMVC.ViewModels.Seguimientos
{
    public class CrearLeadViewModel
    {
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(200)]
        [Display(Name = "Nombre del Contacto")]
        public string NombreContacto { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [MaxLength(20)]
        [Display(Name = "Teléfono")]
        [Phone(ErrorMessage = "Formato de teléfono no válido")]
        public string Telefono { get; set; } = string.Empty;

        [MaxLength(200)]
        [EmailAddress(ErrorMessage = "Correo no válido")]
        [Display(Name = "Correo Electrónico")]
        public string? CorreoElectronico { get; set; }

        [MaxLength(300)]
        [Display(Name = "Empresa del Cliente")]
        public string? EmpresaCliente { get; set; }

        [MaxLength(100)]
        [Display(Name = "Producto de Interés")]
        public string? ProductoBusca { get; set; }

        [MaxLength(500)]
        [Display(Name = "Comentarios")]
        [DataType(DataType.MultilineText)]
        public string? Comentarios { get; set; }
    }
}