namespace CotizacionMVC.ViewModels.Cotizacion
{
    public class CotizacionIndiceViewModel
    {
        public List<CotizacionResumenViewModel> Cotizaciones { get; set; } = new();
        public List<LeadResumenViewModel> Leads { get; set; } = new();
    }
}