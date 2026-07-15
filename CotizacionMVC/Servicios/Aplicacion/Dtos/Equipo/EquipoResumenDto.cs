using CotizacionMVC.Models.Enums;

namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Equipo
{
    public class EquipoResumenDto
    {
        public Guid Id { get; set; }
        public TipoMarca Marca { get; set; }
        public string Modelo { get; set; } = string.Empty;
        public decimal CapacidadToneladas { get; set; }
        public decimal PrecioBase { get; set; }
        public string MonedaOriginal { get; set; } = string.Empty;
        public bool Activo { get; set; }
        public string Sistema { get; set; } = string.Empty;
        public string Modo { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
    }
}
