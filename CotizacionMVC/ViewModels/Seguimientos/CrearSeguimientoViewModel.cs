using System.ComponentModel.DataAnnotations;

namespace CotizacionMVC.ViewModels.Seguimientos
{
    public class CrearSeguimientoViewModel
    {
        [Display(Name = "Lead")]
        public Guid? LeadId { get; set; }

        [Display(Name = "Cotización")]
        public Guid? CotizacionId { get; set; }

        [Required(ErrorMessage = "La fecha de contacto es obligatoria")]
        [Display(Name = "Fecha de Contacto")]
        [DataType(DataType.DateTime)]
        public DateTime FechaContacto { get; set; } = DateTime.Now;

        [Required(ErrorMessage = "El medio de contacto es obligatorio")]
        [Range(1, 4, ErrorMessage = "Seleccione un medio de contacto válido")]
        [Display(Name = "Medio de Contacto")]
        public int MedioContactoId { get; set; }

        [Required(ErrorMessage = "El resultado es obligatorio")]
        [Display(Name = "¿Qué pasó en la llamada?")]
        public int ResultadoId { get; set; }

        [Display(Name = "Etapa de Negociación")]
        public int? EtapaNegociacionId { get; set; }

        [MaxLength(500, ErrorMessage = "Las notas no pueden exceder 500 caracteres")]
        [Display(Name = "Notas")]
        [DataType(DataType.MultilineText)]
        public string? Notas { get; set; }

        [Display(Name = "Próximo Contacto")]
        [DataType(DataType.DateTime)]
        [FechaMayorQueActual(ErrorMessage = "La fecha debe ser futura")]
        public DateTime? ProximoContacto { get; set; }

        public string? Referencia { get; set; }
        public string? TipoSeguimiento { get; set; }
        public bool EsSeguimientoGeneral => !LeadId.HasValue && !CotizacionId.HasValue;
        public string? ClienteNombre { get; set; }
        public string? ClienteTelefono { get; set; }
        public string? ClienteCorreo { get; set; }
        public string? EtapaActual { get; set; }
        public string? NumeroCotizacion { get; set; }
        public decimal? MontoCotizacion { get; set; }
        public string? Origen { get; set; }
    }
}