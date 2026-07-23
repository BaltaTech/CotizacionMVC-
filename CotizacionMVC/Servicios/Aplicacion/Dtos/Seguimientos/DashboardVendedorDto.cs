namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Seguimientos
{
    public class DashboardVendedorDto
    {
        // Métricas actuales
        public int LeadsSinContactar { get; set; }
        public int LeadsFriosSinActividad { get; set; }
        public int SeguimientosPendientesHoy { get; set; }
        public int SeguimientosVencidos { get; set; }
        public int SeguimientosRealizadosHoy { get; set; }
        public int CotizacionesActivas { get; set; }
        public int LeadsCalificadosSinCotizar { get; set; }

        // Nuevas métricas separadas
        public int RecepcionSinContactar { get; set; }
        public int ProspeccionSinContactar { get; set; }
        public int UrgentesSinContactar { get; set; }
        public int EnNegociacion { get; set; }
        public int VendidasMes { get; set; }
        public decimal MontoVendidoMes { get; set; }

        public List<SeguimientoListaDto> ProximosSeguimientos { get; set; } = new();
    }
}