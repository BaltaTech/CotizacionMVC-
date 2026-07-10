namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Cotizacion
{
    public class ResultadoCotizacionDto
    {
        public bool Exitoso { get; set; }
        public string? MensajeError { get; set; }
        public CotizacionDetalleDto? Cotizacion { get; set; }

        public static ResultadoCotizacionDto Exito(CotizacionDetalleDto cotizacion)
        {
            return new ResultadoCotizacionDto { Exitoso = true, Cotizacion = cotizacion };
        }

        public static ResultadoCotizacionDto Error(string mensaje)
        {
            return new ResultadoCotizacionDto { Exitoso = false, MensajeError = mensaje };
        }
    }
}