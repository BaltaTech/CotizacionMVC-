using CotizacionMVC.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace CotizacionMVC.ViewModels.Seguimientos
{
    public class CrearSeguimientoViewModel
    {
        public Guid? LeadId { get; set; }
        public Guid? CotizacionId { get; set; }

        [Required(ErrorMessage = "La fecha de contacto es obligatoria")]
        public DateTime FechaContacto { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El medio de contacto es obligatorio")]
        public MedioContacto MedioContacto { get; set; }

        [Required(ErrorMessage = "El resultado es obligatorio")]
        public ResultadoSeguimiento Resultado { get; set; }

        [MaxLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
        public string? Notas { get; set; }

        public DateTime? ProximoContacto { get; set; }
    }
}