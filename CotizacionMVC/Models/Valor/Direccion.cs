using System.Text.RegularExpressions;

namespace CotizacionMVC.Models.Valor
{
    public class Direccion
    {
        public string Calle { get; private set; }
        public string? NumeroExterior { get; private set; }  // ← Puede ser null
        public string? NumeroInterior { get; private set; }  // ← Puede ser null
        public string Colonia { get; private set; }
        public string Ciudad { get; private set; }
        public string? Estado { get; private set; }  // ← Puede ser null
        public string CodigoPostal { get; private set; }

        // Constructor privado para EF Core
        private Direccion()
        {
            Calle = null!;
            Colonia = null!;
            Ciudad = null!;
            CodigoPostal = null!;
            NumeroExterior = null;
            NumeroInterior = null;
            Estado = null;
        }

        public Direccion(
            string calle,
            string? numeroExterior,
            string colonia,
            string ciudad,
            string? estado,
            string codigoPostal,
            string? numeroInterior = null)
        {
            if (string.IsNullOrWhiteSpace(calle))
                throw new ArgumentException("La calle es obligatoria", nameof(calle));

            if (string.IsNullOrWhiteSpace(colonia))
                throw new ArgumentException("La colonia es obligatoria", nameof(colonia));

            if (string.IsNullOrWhiteSpace(ciudad))
                throw new ArgumentException("La ciudad es obligatoria", nameof(ciudad));

            if (string.IsNullOrWhiteSpace(codigoPostal))
                throw new ArgumentException("El código postal es obligatorio", nameof(codigoPostal));

            if (!EsCodigoPostalValido(codigoPostal))
                throw new ArgumentException("El código postal debe tener 5 dígitos");

            Calle = calle.Trim();
            NumeroExterior = numeroExterior?.Trim();
            NumeroInterior = numeroInterior?.Trim();
            Colonia = colonia.Trim();
            Ciudad = ciudad.Trim();
            Estado = estado?.Trim();
            CodigoPostal = codigoPostal.Trim();
        }

        private bool EsCodigoPostalValido(string cp)
        {
            return Regex.IsMatch(cp, @"^\d{5}$");
        }

        public string DireccionCompleta()
        {
            var direccion = Calle;

            // Agregar número exterior si existe
            if (!string.IsNullOrWhiteSpace(NumeroExterior))
                direccion += $" {NumeroExterior}";
            else
                direccion += " S/N";

            // Agregar número interior si existe
            if (!string.IsNullOrWhiteSpace(NumeroInterior))
                direccion += $" Int. {NumeroInterior}";

            direccion += $", {Colonia}, {Ciudad}";

            // Agregar estado si existe
            if (!string.IsNullOrWhiteSpace(Estado))
                direccion += $", {Estado}";

            direccion += $", CP {CodigoPostal}";
            return direccion;
        }

        public string DireccionCorta()
        {
            var direccion = Calle;

            if (!string.IsNullOrWhiteSpace(NumeroExterior))
                direccion += $" {NumeroExterior}";
            else
                direccion += " S/N";

            if (!string.IsNullOrWhiteSpace(NumeroInterior))
                direccion += $" Int. {NumeroInterior}";

            direccion += $", {Colonia}";
            return direccion;
        }

        public override string ToString()
        {
            return DireccionCompleta();
        }

        // Método para validar si la dirección está completa
        public bool EsCompleta()
        {
            return !string.IsNullOrWhiteSpace(Calle) &&
                   !string.IsNullOrWhiteSpace(Colonia) &&
                   !string.IsNullOrWhiteSpace(Ciudad) &&
                   !string.IsNullOrWhiteSpace(CodigoPostal);
        }
    }
}