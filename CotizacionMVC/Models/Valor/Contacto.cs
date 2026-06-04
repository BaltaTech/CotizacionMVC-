using System.Text.RegularExpressions;

namespace CotizacionMVC.Models.Valor
{
    public class Contacto
    {
        public string? Telefono { get; private set; }
        public string? TelefonoMovil { get; private set; }
        public string? Correo { get; private set; }
        public string? NombreContacto { get; private set; }

        // Constructor para EF Core (necesita un constructor sin parámetros)
        private Contacto() { }

        public Contacto(string? telefono, string? telefonoMovil, string? correo, string? nombreContacto)
        {
            bool tieneTelefono = !string.IsNullOrWhiteSpace(telefono);
            bool tieneMovil = !string.IsNullOrWhiteSpace(telefonoMovil);
            bool tieneCorreo = !string.IsNullOrWhiteSpace(correo);

            if (!tieneTelefono && !tieneMovil && !tieneCorreo)
                throw new ArgumentException("Debe proporcionar al menos un medio de contacto (teléfono, móvil o correo)");

            if (!string.IsNullOrWhiteSpace(correo) && !EsCorreoValido(correo))
                throw new ArgumentException("El correo electrónico no tiene un formato válido");

            if (!string.IsNullOrWhiteSpace(telefono) && telefono.Length < 10)
                throw new ArgumentException("El teléfono debe tener al menos 10 dígitos");

            if (!string.IsNullOrWhiteSpace(telefonoMovil) && telefonoMovil.Length < 10)
                throw new ArgumentException("El teléfono móvil debe tener al menos 10 dígitos");

            Telefono = telefono?.Trim();
            TelefonoMovil = telefonoMovil?.Trim();
            Correo = correo?.Trim().ToLower();
            NombreContacto = nombreContacto?.Trim();
        }

        private bool EsCorreoValido(string correo)
        {
            var patron = @"^[^@\s]+@[^@\s]+\.[^@\s]+$";
            return Regex.IsMatch(correo, patron);
        }

        public Contacto ConNombreContacto(string nombre)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre de contacto es obligatorio", nameof(nombre));

            return new Contacto(Telefono, TelefonoMovil, Correo, nombre);
        }

        public override string ToString()
        {
            return NombreContacto ?? Correo ?? Telefono ?? TelefonoMovil ?? "Sin contacto";
        }

        // Método para saber si el contacto tiene al menos un medio
        public bool TieneMedioDeContacto()
        {
            return !string.IsNullOrWhiteSpace(Telefono) ||
                   !string.IsNullOrWhiteSpace(TelefonoMovil) ||
                   !string.IsNullOrWhiteSpace(Correo);
        }

        // Método para obtener el medio principal de contacto
        public string ObtenerMedioPrincipal()
        {
            if (!string.IsNullOrWhiteSpace(Telefono))
                return Telefono;
            if (!string.IsNullOrWhiteSpace(TelefonoMovil))
                return TelefonoMovil;
            if (!string.IsNullOrWhiteSpace(Correo))
                return Correo;
            return "Sin medio de contacto";
        }
    }
}