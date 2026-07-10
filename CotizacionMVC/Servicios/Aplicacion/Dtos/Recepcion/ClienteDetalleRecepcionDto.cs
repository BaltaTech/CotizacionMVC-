namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Recepcion
{
    public class ClienteDetalleRecepcionDto
    {
        public string Folio { get; set; } = string.Empty;
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? TelefonoMovil { get; set; }
        public string? Correo { get; set; }
        public string Origen { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
        public string? Producto { get; set; }
        public string? VendedorNombre { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string? Observaciones { get; set; }
    }
}
