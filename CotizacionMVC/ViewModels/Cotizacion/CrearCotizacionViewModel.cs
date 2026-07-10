using System.ComponentModel.DataAnnotations;

namespace CotizacionMVC.ViewModels.Cotizacion
{
    public class CrearCotizacionViewModel
    {
        [Required]
        public Guid ClienteId { get; set; }

        [Range(1, double.MaxValue, ErrorMessage = "El área debe ser mayor a cero")]
        public decimal AreaMetrosCuadrados { get; set; }

        public string CondicionesPago { get; set; } = string.Empty;
        public string EquiposJson { get; set; } = string.Empty;
        public string InstalacionesJson { get; set; } = string.Empty;
        public Guid? LeadId { get; set; }
    }
}