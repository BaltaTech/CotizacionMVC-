namespace CotizacionMVC.Models.Valor
{
    public class Dinero
    {
        public decimal Monto { get; private set; }
        public string Moneda { get; private set; }
        public decimal? TipoCambio { get; private set; }

        // Constructor privado para EF Core
        private Dinero()
        {
            Moneda = null!;
        }

        public Dinero(decimal monto, string moneda, decimal? tipoCambio = null)
        {
            if (monto < 0)
                throw new ArgumentException("El monto no puede ser negativo", nameof(monto));

            if (moneda != "MXN" && moneda != "USD")
                throw new ArgumentException("La moneda debe ser MXN o USD", nameof(moneda));

            Monto = Math.Round(monto, 2);
            Moneda = moneda;
            TipoCambio = tipoCambio;
        }

        public Dinero ConvertirA(string monedaDestino, decimal tipoCambioActual)
        {
            if (string.IsNullOrWhiteSpace(monedaDestino))
                throw new ArgumentException("La moneda destino es obligatoria", nameof(monedaDestino));

            if (monedaDestino != "MXN" && monedaDestino != "USD")
                throw new ArgumentException("La moneda destino debe ser MXN o USD", nameof(monedaDestino));

            if (Moneda == monedaDestino)
                return new Dinero(Monto, Moneda);

            if (tipoCambioActual <= 0)
                throw new ArgumentException("El tipo de cambio debe ser mayor a cero");

            decimal nuevoMonto = Moneda == "USD"
                ? Monto * tipoCambioActual
                : Monto / tipoCambioActual;

            return new Dinero(nuevoMonto, monedaDestino, tipoCambioActual);
        }

        public Dinero Sumar(Dinero otro)
        {
            if (otro == null)
                throw new ArgumentNullException(nameof(otro));

            if (otro.Moneda != Moneda)
                throw new InvalidOperationException($"No se pueden sumar monedas diferentes: {Moneda} y {otro.Moneda}");

            return new Dinero(Monto + otro.Monto, Moneda);
        }

        public Dinero Multiplicar(decimal factor)
        {
            if (factor < 0)
                throw new ArgumentException("El factor no puede ser negativo");

            return new Dinero(Monto * factor, Moneda);
        }

        public bool EsMismoValorQue(Dinero otro, decimal tolerancia = 0.01m)
        {
            if (otro == null)
                return false;

            if (Moneda != otro.Moneda)
                return false;

            return Math.Abs(Monto - otro.Monto) <= tolerancia;
        }

        public Dinero Restar(Dinero otro)
        {
            if (otro == null)
                throw new ArgumentNullException(nameof(otro));

            if (otro.Moneda != Moneda)
                throw new InvalidOperationException($"No se pueden restar monedas diferentes: {Moneda} y {otro.Moneda}");

            var resultado = Monto - otro.Monto;

            if (resultado < 0)
                throw new InvalidOperationException("El resultado de la resta no puede ser negativo");

            return new Dinero(resultado, Moneda);
        }

        public override bool Equals(object? obj)
        {
            if (obj == null)
                return false;

            if (obj is not Dinero otro)
                return false;

            return Monto == otro.Monto && Moneda == otro.Moneda;
        }

        public override int GetHashCode()
        {
            return HashCode.Combine(Monto, Moneda);
        }

        public override string ToString()
        {
            return $"{Moneda} {Monto:N2}";
        }
        public static bool operator ==(Dinero? izquierdo, Dinero? derecho)
        {
            if (ReferenceEquals(izquierdo, derecho))
                return true;

            if (izquierdo is null || derecho is null)
                return false;

            return izquierdo.Equals(derecho);
        }

        public static bool operator !=(Dinero? izquierdo, Dinero? derecho)
        {
            return !(izquierdo == derecho);
        }

        public static Dinero operator +(Dinero izquierdo, Dinero derecho)
        {
            if (izquierdo is null)
                throw new ArgumentNullException(nameof(izquierdo));

            return izquierdo.Sumar(derecho);
        }

        public static Dinero operator *(Dinero dinero, decimal factor)
        {
            if (dinero is null)
                throw new ArgumentNullException(nameof(dinero));

            return dinero.Multiplicar(factor);
        }
    }
}