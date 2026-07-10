namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Recepcion
{
    public class VendedorResumenDto
    {
        public Guid Id { get; set; }
        public string NombreCompleto { get; set; } = string.Empty;
        public string? Email { get; set; }
    }
}
