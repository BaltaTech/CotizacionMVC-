namespace CotizacionMVC.ViewModels.Cotizacion
{
    public class LeadResumenViewModel
    {
        public Guid Id { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public Guid? ClienteId { get; set; }
        public string? Telefono { get; set; }
        public string? ProductoBusca { get; set; }
        public string EmpresaNombre { get; set; } = string.Empty;
        public int Estado { get; set; }
        public DateTime? FechaAsignacion { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string? NombreContacto { get; set; }
        public string? ClienteTelefono { get; set; }
        public int OrigenLead { get; set; }
    }
}