namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Recepcion
{
    public class UltimoRegistroDto
    {
        public Guid Id { get; set; }
        public string Folio { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string Origen { get; set; } = string.Empty;
        public DateTime Fecha { get; set; }
        public string Estado { get; set; } = string.Empty;
        public Guid? VendedorAsignadoId { get; set; }
        public string? Observaciones { get; set; }
        public string FechaFormateada => Fecha.ToString("dd/MM/yyyy HH:mm");
    }
}
