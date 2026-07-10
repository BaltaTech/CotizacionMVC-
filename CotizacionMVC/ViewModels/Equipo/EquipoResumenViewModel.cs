using CotizacionMVC.Models.Enums;

namespace CotizacionMVC.ViewModels.Equipo
{
    public class EquipoResumenViewModel
    {
        public Guid Id { get; set; }
        public TipoMarca Marca { get; set; }
        public string Modelo { get; set; } = string.Empty;
        public decimal CapacidadToneladas { get; set; }
        public decimal PrecioBase { get; set; }
        public string MonedaOriginal { get; set; } = string.Empty;
        public bool Activo { get; set; }
    }
}
