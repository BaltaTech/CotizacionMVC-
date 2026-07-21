using System.Text.RegularExpressions;

namespace CotizacionMVC.Models.Valor
{
    public class Direccion
    {
        public string? Calle { get; private set; }
        public string? NumeroExterior { get; private set; }
        public string? NumeroInterior { get; private set; }
        public string? Colonia { get; private set; }
        public string? Ciudad { get; private set; }
        public string? Estado { get; private set; }
        public string CodigoPostal { get; private set; }

        // Constructor privado para EF Core
        private Direccion()
        {
            CodigoPostal = null!;
        }

        public Direccion(
            string? calle,
            string? numeroExterior,
            string? colonia,
            string? ciudad,
            string? estado,
            string codigoPostal,
            string? numeroInterior = null)
        {
            if (string.IsNullOrWhiteSpace(codigoPostal))
                throw new ArgumentException("El código postal es obligatorio", nameof(codigoPostal));

            if (!EsCodigoPostalValido(codigoPostal))
                throw new ArgumentException("El código postal debe tener 5 dígitos");

            Calle = calle?.Trim();
            NumeroExterior = numeroExterior?.Trim();
            NumeroInterior = numeroInterior?.Trim();
            Colonia = colonia?.Trim();
            Ciudad = ciudad?.Trim();
            Estado = estado?.Trim();
            CodigoPostal = codigoPostal.Trim();
        }

        private bool EsCodigoPostalValido(string cp)
        {
            return Regex.IsMatch(cp, @"^\d{5}$");
        }

        public string DireccionCompleta()
        {
            var partes = new List<string>();

            if (!string.IsNullOrWhiteSpace(Calle))
            {
                var calleNumero = Calle;
                if (!string.IsNullOrWhiteSpace(NumeroExterior))
                    calleNumero += $" {NumeroExterior}";
                if (!string.IsNullOrWhiteSpace(NumeroInterior))
                    calleNumero += $" Int. {NumeroInterior}";
                partes.Add(calleNumero);
            }

            if (!string.IsNullOrWhiteSpace(Colonia))
                partes.Add(Colonia);

            if (!string.IsNullOrWhiteSpace(Ciudad))
                partes.Add(Ciudad);

            if (!string.IsNullOrWhiteSpace(Estado))
                partes.Add(Estado);

            partes.Add($"CP {CodigoPostal}");

            return string.Join(", ", partes);
        }

        public string DireccionCorta()
        {
            var partes = new List<string>();

            if (!string.IsNullOrWhiteSpace(Calle))
            {
                var calleNumero = Calle;
                if (!string.IsNullOrWhiteSpace(NumeroExterior))
                    calleNumero += $" {NumeroExterior}";
                partes.Add(calleNumero);
            }

            if (!string.IsNullOrWhiteSpace(Colonia))
                partes.Add(Colonia);

            if (!string.IsNullOrWhiteSpace(Ciudad))
                partes.Add(Ciudad);

            return string.Join(", ", partes);
        }

        public override string ToString()
        {
            return DireccionCompleta();
        }

        public bool EsCompleta()
        {
            return !string.IsNullOrWhiteSpace(Calle) &&
                   !string.IsNullOrWhiteSpace(Colonia) &&
                   !string.IsNullOrWhiteSpace(Ciudad) &&
                   !string.IsNullOrWhiteSpace(CodigoPostal);
        }
        public bool EsParcial()
        {
            return !string.IsNullOrWhiteSpace(CodigoPostal) && !EsCompleta();
        }
    }
}