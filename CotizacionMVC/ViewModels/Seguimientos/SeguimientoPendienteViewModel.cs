namespace CotizacionMVC.ViewModels.Seguimientos
{
    public class SeguimientoPendienteViewModel
    {
        public Guid Id { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string? Tipo { get; set; }
        public string? MedioContacto { get; set; }
        public string? Notas { get; set; }
        public DateTime FechaProgramada { get; set; }
        public Guid? LeadId { get; set; }
        public Guid? CotizacionId { get; set; }

    }
}
