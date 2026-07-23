namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Seguimientos
{
    public class SeguimientoListaDto
    {
        public Guid Id { get; set; }
        public DateTime FechaContacto { get; set; }
        public string MedioContacto { get; set; } = string.Empty;
        public string Resultado { get; set; } = string.Empty;
        public string? Notas { get; set; }
        public DateTime? ProximoContacto { get; set; }
        public string VendedorNombre { get; set; } = string.Empty;
        public string? LeadNombre { get; set; }
        public string? CotizacionNumero { get; set; }
        public bool EsDeLead { get; set; }
        public bool EsDeCotizacion { get; set; }
        public Guid? LeadId { get; set; }
        public Guid? CotizacionId { get; set; }
        public string? Telefono { get; set; }
        public string? CorreoElectronico { get; set; }
        public string? EtapaNegociacion { get; set; }
        public string? AlcanceVenta { get; set; }
    }
}
