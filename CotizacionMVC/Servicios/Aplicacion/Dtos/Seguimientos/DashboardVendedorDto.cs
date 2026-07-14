namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Seguimientos
{
    public class DashboardVendedorDto
    {
        public int LeadsSinContactar { get; set; }
        public int LeadsFriosSinActividad { get; set; }
        public int SeguimientosPendientesHoy { get; set; }
        public int SeguimientosVencidos { get; set; }
        public int SeguimientosRealizadosHoy { get; set; }
        public int CotizacionesActivas { get; set; }
        public int LeadsCalificadosSinCotizar { get; set; }
        public List<SeguimientoListaDto> ProximosSeguimientos { get; set; } = new();
    }
}
