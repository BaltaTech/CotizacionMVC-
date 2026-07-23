namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Seguimientos
{
    public class CrearSeguimientoDto
    {
        public Guid? LeadId { get; set; }
        public Guid? CotizacionId { get; set; }
        public Guid VendedorId { get; set; }
        public DateTime FechaContacto { get; set; }
        public int MedioContacto { get; set; }
        public int Resultado { get; set; }
        public string? Notas { get; set; }
        public DateTime? ProximoContacto { get; set; }
        public int? EtapaNegociacion { get; set; }

    }
}
