using System.ComponentModel.DataAnnotations;

namespace CotizacionMVC.ViewModels.Usuarios
{
    public class CrearUsuarioViewModel
    {
        [Required(ErrorMessage = "El nombre completo es obligatorio")]
        [Display(Name = "Nombre Completo")]
        public string NombreCompleto { get; set; } = "";

        [Required(ErrorMessage = "El correo electrónico es obligatorio")]
        [EmailAddress(ErrorMessage = "Correo electrónico no válido")]
        [Display(Name = "Correo Electrónico")]
        public string Email { get; set; } = "";

        [Required(ErrorMessage = "La contraseña es obligatoria")]
        [MinLength(6, ErrorMessage = "La contraseña debe tener al menos 6 caracteres")]
        [DataType(DataType.Password)]
        [Display(Name = "Contraseña")]
        public string Password { get; set; } = "";

        [Display(Name = "Rol")]
        public string? Rol { get; set; }

        [Required(ErrorMessage = "Debe seleccionar una empresa")]
        [Display(Name = "Empresa")]
        public Guid EmpresaId { get; set; }
    }
}