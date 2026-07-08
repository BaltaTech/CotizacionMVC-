using CotizacionMVC.Models.Entidades;

namespace CotizacionMVC.Servicios.Aplicacion
{
    public class ResultadoCrearCotizacion
    {
        public bool Exitoso { get; private set; }
        public string? MensajeError { get; private set; }
        public Cotizacion? Cotizacion { get; private set; }

        public static ResultadoCrearCotizacion Exito(Cotizacion cotizacion)
        {
            return new ResultadoCrearCotizacion
            {
                Exitoso = true,
                Cotizacion = cotizacion
            };
        }

        public static ResultadoCrearCotizacion Error(string mensaje)
        {
            return new ResultadoCrearCotizacion
            {
                Exitoso = false,
                MensajeError = mensaje
            };
        }
    }
}
