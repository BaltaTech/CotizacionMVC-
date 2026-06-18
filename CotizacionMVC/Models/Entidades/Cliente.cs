using CotizacionMVC.Models.Valor;

namespace CotizacionMVC.Models.Entidades
{
    public class Cliente
    {
        private readonly List<Cotizacion> _cotizaciones = new List<Cotizacion>();

        public Guid Id { get; private set; }
        public string Nombre { get; private set; }
        public Contacto Contacto { get; private set; }
        public Direccion? Direccion { get; private set; }  
        public DateTime FechaRegistro { get; private set; }
        public string? Observaciones { get; private set; }

        public IReadOnlyCollection<Cotizacion> Cotizaciones => _cotizaciones.AsReadOnly();

        // Constructor protegido para EF Core
        protected Cliente()
        {
            Nombre = null!;
            Contacto = null!;
            Direccion = null;
            Observaciones = null;
        }

        public Cliente(string nombre, Contacto contacto)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del cliente es obligatorio");

            if (contacto == null)
                throw new ArgumentNullException(nameof(contacto));

            Id = Guid.NewGuid();
            Nombre = nombre.Trim();
            Contacto = contacto;
            FechaRegistro = DateTime.UtcNow;
            Direccion = null;
            Observaciones = null;
        }

        public void ActualizarContacto(Contacto nuevoContacto)
        {
            if (nuevoContacto == null)
                throw new ArgumentNullException(nameof(nuevoContacto));

            Contacto = nuevoContacto;
        }

        public void ActualizarDireccion(Direccion? direccion)
        {
            Direccion = direccion;
        }

        public void AgregarObservaciones(string? observaciones)
        {
            if (!string.IsNullOrWhiteSpace(observaciones))
                Observaciones = observaciones.Trim();
        }

        // Método para verificar si el cliente tiene dirección registrada
        public bool TieneDireccion()
        {
            return Direccion != null && Direccion.EsCompleta();
        }

        // Método para obtener la dirección completa o un mensaje por defecto
        public string ObtenerDireccionMostrable()
        {
            if (TieneDireccion())
                return Direccion!.DireccionCompleta();

            return "Sin dirección registrada";
        }

        // Método para obtener el medio de contacto principal
        public string ObtenerContactoPrincipal()
        {
            return Contacto.ObtenerMedioPrincipal();
        }

        // Método para saber si el cliente tiene al menos un medio de contacto
        public bool TieneContacto()
        {
            return Contacto.TieneMedioDeContacto();
        }
    }
}