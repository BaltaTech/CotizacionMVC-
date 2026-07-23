namespace CotizacionMVC.Servicios.Aplicacion.Dtos.AdminDashboard
{
    public class AdminDashboardDto
    {
        public int ClientesNuevosHoy { get; set; }
        public int ClientesSinAsignar { get; set; }
        public int CotizacionesEnviadasHoy { get; set; }
        public int CotizacionesActivas { get; set; }
        public int VentasMes { get; set; }
        public decimal MontoVendidoMes { get; set; }
        public int LeadsPerdidosMes { get; set; }
        public decimal TasaConversion { get; set; }

        public List<VendedorMetricasDto> Vendedores { get; set; } = new();
        public List<PipelineEtapaDto> Pipeline { get; set; } = new();
        public List<string> Alertas { get; set; } = new();
    }
}