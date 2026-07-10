using CotizacionMVC.Models.Enums;
using System.ComponentModel.DataAnnotations;

namespace CotizacionMVC.ViewModels.Equipo
{
    public class EquipoFormViewModel
    {
        public Guid? Id { get; set; }

        [Required(ErrorMessage = "La marca es obligatoria")]
        public TipoMarca Marca { get; set; }

        [Required(ErrorMessage = "El modelo es obligatorio")]
        public string Modelo { get; set; } = string.Empty;

        public string? Tipo { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "La capacidad debe ser mayor a cero")]
        public decimal CapacidadToneladas { get; set; }

        public string? Tension { get; set; }
        public string? Tecnologia { get; set; }

        [Range(0.01, double.MaxValue, ErrorMessage = "El precio base debe ser mayor a cero")]
        public decimal PrecioBase { get; set; }

        [Required(ErrorMessage = "La moneda es obligatoria")]
        public string MonedaOriginal { get; set; } = string.Empty;
    }
}