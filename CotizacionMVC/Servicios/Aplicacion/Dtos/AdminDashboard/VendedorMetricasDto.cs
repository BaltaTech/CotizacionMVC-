namespace CotizacionMVC.Servicios.Aplicacion.Dtos.AdminDashboard
{
    public class VendedorMetricasDto
    {
        public Guid VendedorId { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public int TotalLeads { get; set; }
        public int SinContactar { get; set; }
        public int CotizacionesActivas { get; set; }
        public int EnviadasHoy { get; set; }
        public int VendidasMes { get; set; }
        public decimal MontoVendidoMes { get; set; }
        public decimal TasaConversion { get; set; }
        public DateTime? UltimoSeguimiento { get; set; }
    }
}