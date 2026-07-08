using CotizacionMVC.Models.Entidades;

namespace CotizacionMVC.Servicios.Aplicacion
{
    public class ResultadoRegistroCliente
    {
        public bool Exitoso { get; private set; }
        public bool EsDuplicado { get; private set; }
        public string? MensajeError { get; private set; }
        public Cliente? Cliente { get; private set; }
        public Cliente? ClienteExistente { get; private set; }

        public static ResultadoRegistroCliente Exito(Cliente cliente)
        {
            return new ResultadoRegistroCliente
            {
                Exitoso = true,
                Cliente = cliente
            };
        }

        public static ResultadoRegistroCliente Error(string mensaje)
        {
            return new ResultadoRegistroCliente
            {
                Exitoso = false,
                MensajeError = mensaje
            };
        }

        public static ResultadoRegistroCliente Duplicado(Cliente clienteExistente, string mensaje)
        {
            return new ResultadoRegistroCliente
            {
                Exitoso = false,
                EsDuplicado = true,
                MensajeError = mensaje,
                ClienteExistente = clienteExistente
            };
        }
    }
}