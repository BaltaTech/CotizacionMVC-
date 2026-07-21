namespace CotizacionMVC.ViewModels.Seguimientos
{
    public class DashboardViewModel
    {  
        public int LeadsSinContactar { get; set; }
        public int SeguimientosPendientesHoy { get; set; }
        public int CotizacionesActivas { get; set; }
        public int SeguimientosRealizadosHoy { get; set; }
        public int LeadsFriosSinActividad { get; set; }
        public List<SeguimientoPendienteViewModel> ProximosSeguimientos { get; set; } = new();
        public List<SeguimientoRecienteViewModel> UltimosSeguimientos { get; set; } = new();
        public int TotalLeadsActivos { get; set; }
        public int TotalCotizacionesPendientes { get; set; }
    }

   
   
}