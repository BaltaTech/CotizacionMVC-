namespace CotizacionMVC.ViewModels.Cliente
{
    public class ClienteIndiceViewModel
    {
        public List<ClienteResumenViewModel> Clientes { get; set; } = new();
        public string? TerminoBusqueda { get; set; }
    }
}
