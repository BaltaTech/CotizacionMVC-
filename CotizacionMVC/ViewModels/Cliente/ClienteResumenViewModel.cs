using CotizacionMVC.Models.Enums;

namespace CotizacionMVC.ViewModels.Cliente
{
    public class ClienteResumenViewModel
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string Telefono { get; set; } = string.Empty;
        public string Correo { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string Empresa { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public int CantidadCotizaciones { get; set; }
        public DateTime? UltimaFechaSeguimiento { get; set; }
        public DateTime? ProximaFechaSeguimiento { get; set; }
        public int DiasSinActividad { get; set; }
        public decimal TotalUltimaCotizacion { get; set; }
        public string Moneda { get; set; } = string.Empty;
        public bool TieneSeguimientoHoy { get; set; }
        public bool EsCaliente { get; set; }
    }
}
