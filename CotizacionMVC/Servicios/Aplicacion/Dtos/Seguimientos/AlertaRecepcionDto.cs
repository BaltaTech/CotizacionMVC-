namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Seguimientos
{
    public class AlertaRecepcionDto
    {
        public string Tipo { get; set; } = string.Empty;
        public string Mensaje { get; set; } = string.Empty;
        public string ClienteNombre { get; set; } = string.Empty;
        public string Folio { get; set; } = string.Empty;
        public string? VendedorNombre { get; set; }
    }
}
