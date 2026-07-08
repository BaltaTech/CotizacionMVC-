using CotizacionMVC.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace CotizacionMVC.ViewModels.Recepcion
{
    public class RegistrarClienteViewModel
    {
        // Empresa
        [Required(ErrorMessage = "Debe seleccionar una empresa")]
        public Guid EmpresaId { get; set; }

        // Datos del cliente
        [Required(ErrorMessage = "El nombre es obligatorio")]
        [MaxLength(300)]
        public string Nombre { get; set; } = string.Empty;

        [Required(ErrorMessage = "El teléfono es obligatorio")]
        [MaxLength(20)]
        public string Telefono { get; set; } = string.Empty;

        [MaxLength(200)]
        [EmailAddress(ErrorMessage = "Correo no válido")]
        public string? Correo { get; set; }

        [MaxLength(200)]
        public string? EmpresaCliente { get; set; }

        // Código Postal (NUEVO - principal)
        [Required(ErrorMessage = "El código postal es obligatorio")]
        [MaxLength(10)]
        public string CodigoPostal { get; set; } = string.Empty;

        // Ciudad (ahora opcional)
        [MaxLength(200)]
        public string? Ciudad { get; set; }

        // Producto
        [Required(ErrorMessage = "Debe seleccionar un producto")]
        [MaxLength(100)]
        public string ProductoBusca { get; set; } = string.Empty;

        [MaxLength(500)]
        public string? Comentarios { get; set; }

        // Origen
        public OrigenCliente Origen { get; set; } = OrigenCliente.Llamada;

        // Asignación
        public Guid? VendedorAsignadoId { get; set; }
        public bool AsignarAhora { get; set; } = true;
    }
}