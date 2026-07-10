using CotizacionMVC.Models.Enums;

namespace CotizacionMVC.ViewModels.Cotizacion
{
    public class CotizacionResumenViewModel
    {
        public Guid Id { get; set; }
        public string NumeroCotizacion { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string EmpresaNombre { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public decimal Total { get; set; }
        public string Moneda { get; set; } = string.Empty;
        public EstadoCotizacion Estado { get; set; }
    }
}