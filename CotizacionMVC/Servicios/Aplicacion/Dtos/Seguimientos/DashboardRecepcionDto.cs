namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Seguimientos
{
    public class DashboardRecepcionDto
    {
        public int NuevosClientesHoy { get; set; }
        public int ClientesSinAsignar { get; set; }
        public int ClientesAsignadosSinContactar { get; set; }
        public int ClientesContactados { get; set; }
        public int ClientesCotizados { get; set; }
        public int ClientesEnNegociacion { get; set; }
        public int ClientesCerrados { get; set; }
        public int ClientesPerdidos { get; set; }
        public int CotizadosHoy { get; set; }
        public List<AlertaRecepcionDto> Alertas { get; set; } = new();
    }
}
