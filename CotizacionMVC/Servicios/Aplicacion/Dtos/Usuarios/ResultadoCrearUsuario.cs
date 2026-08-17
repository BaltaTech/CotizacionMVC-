namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Usuarios
{
    public class ResultadoCrearUsuario
    {
        public bool Exitoso { get; set; }
        public string? MensajeError { get; set; }
        public Guid? UsuarioId { get; set; }
        public string? Nombre { get; set; }
        public string? Email { get; set; }

        public static ResultadoCrearUsuario Exito(Guid usuarioId, string nombre, string email)
        {
            return new ResultadoCrearUsuario
            {
                Exitoso = true,
                UsuarioId = usuarioId,
                Nombre = nombre,
                Email = email
            };
        }

        public static ResultadoCrearUsuario Error(string mensaje)
        {
            return new ResultadoCrearUsuario
            {
                Exitoso = false,
                MensajeError = mensaje
            };
        }
    }

    public class ResultadoOperacion
    {
        public bool Exitoso { get; set; }
        public string? MensajeError { get; set; }

        public static ResultadoOperacion Exito()
        {
            return new ResultadoOperacion { Exitoso = true };
        }

        public static ResultadoOperacion Error(string mensaje)
        {
            return new ResultadoOperacion { Exitoso = false, MensajeError = mensaje };
        }
    }
}