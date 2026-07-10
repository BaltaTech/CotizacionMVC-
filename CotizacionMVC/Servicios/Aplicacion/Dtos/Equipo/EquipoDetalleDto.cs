using CotizacionMVC.Models.Enums;

namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Equipo
{
    public class EquipoDetalleDto
    {
        public Guid Id { get; set; }
        public TipoMarca Marca { get; set; }
        public string Modelo { get; set; } = string.Empty;
        public string? Tipo { get; set; }
        public decimal CapacidadToneladas { get; set; }
        public string? Tension { get; set; }
        public string? Tecnologia { get; set; }
        public decimal PrecioBase { get; set; }
        public string MonedaOriginal { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
