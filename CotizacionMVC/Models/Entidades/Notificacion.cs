// Models/Entidades/Notificacion.cs
namespace CotizacionMVC.Models.Entidades
{
    public class Notificacion
    {
        public Guid Id { get; private set; }
        public Guid UsuarioId { get; private set; }
        public string Titulo { get; private set; }
        public string Mensaje { get; private set; }
        public string Tipo { get; private set; }
        public string? Url { get; private set; }
        public bool Leida { get; private set; }
        public DateTime FechaCreacion { get; private set; }

        private Notificacion()
        {
            Titulo = null!;
            Mensaje = null!;
            Tipo = null!;
        }

        public Notificacion(Guid usuarioId, string titulo, string mensaje, string tipo = "info", string? url = null)
        {
            if (string.IsNullOrWhiteSpace(titulo))
                throw new ArgumentException("El título es obligatorio");
            if (string.IsNullOrWhiteSpace(mensaje))
                throw new ArgumentException("El mensaje es obligatorio");

            Id = Guid.NewGuid();
            UsuarioId = usuarioId;
            Titulo = titulo.Trim();
            Mensaje = mensaje.Trim();
            Tipo = tipo;
            Url = url?.Trim();
            Leida = false;
            FechaCreacion = DateTime.UtcNow;
        }

        public void MarcarLeida() => Leida = true;
        public void MarcarNoLeida() => Leida = false;
    }
}