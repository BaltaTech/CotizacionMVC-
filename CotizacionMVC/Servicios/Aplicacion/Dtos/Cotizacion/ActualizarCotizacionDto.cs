namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Cotizacion
{
    public class ActualizarCotizacionDto
    {
        public Guid Id { get; set; }
        public Guid ClienteId { get; set; }
        public decimal AreaMetrosCuadrados { get; set; }
        public string CondicionesPago { get; set; } = string.Empty;
    }
}