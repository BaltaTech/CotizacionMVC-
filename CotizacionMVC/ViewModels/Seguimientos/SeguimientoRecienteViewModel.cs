namespace CotizacionMVC.ViewModels.Seguimientos
{
    public class SeguimientoRecienteViewModel
    {
        public Guid Id { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public string Resultado { get; set; } = string.Empty;
        public string MedioContacto { get; set; } = string.Empty;
        public DateTime FechaContacto { get; set; }
        public string? Notas { get; set; }
    }
}
