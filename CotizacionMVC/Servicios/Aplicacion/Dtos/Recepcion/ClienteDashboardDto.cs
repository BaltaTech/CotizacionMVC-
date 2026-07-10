namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Recepcion
{
    public class ClienteDashboardDto
    {
        public Guid Id { get; set; }
        public string Folio { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string Origen { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public DateTime FechaRegistro { get; set; }
        public string? Observaciones { get; set; }
        public Guid? VendedorAsignadoId { get; set; }
        public string? VendedorNombre { get; set; }
    }
}
