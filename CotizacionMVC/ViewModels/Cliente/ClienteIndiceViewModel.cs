namespace CotizacionMVC.ViewModels.Cliente
{
    public class ClienteIndiceViewModel
    {
        public IEnumerable<ClienteResumenViewModel> Clientes { get; set; } = new List<ClienteResumenViewModel>();
        public string? TerminoBusqueda { get; set; }
        public string? FiltroActivo { get; set; }
    }
}
