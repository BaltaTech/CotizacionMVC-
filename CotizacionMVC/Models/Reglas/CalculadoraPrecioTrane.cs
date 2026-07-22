namespace CotizacionMVC.Models.Reglas
{
    public class CalculadoraPrecioTrane : ICalculadoraPrecio
    {
        private const decimal FACTOR_PRECIO_DEFAULT = 0.31m;
        private const decimal FACTOR_UTILIDAD_DEFAULT = 1.18m;

        public (decimal precioUSD, decimal precioMXN) Calcular(
            decimal precioBase,
            string monedaOriginal,
            decimal tipoCambio,
            params decimal[] factores)
        {
            // Extraer factores (con valores por defecto si no se proporcionan)
            var factorPrecio = factores.Length > 0 && factores[0] > 0
                ? factores[0]
                : FACTOR_PRECIO_DEFAULT;

            var factorUtilidad = factores.Length > 1 && factores[1] > 0
                ? factores[1]
                : FACTOR_UTILIDAD_DEFAULT;

            // Cálculo en USD (TRANE siempre tiene precios base en USD)
            var precioUSD = precioBase * factorPrecio * factorUtilidad;

            // Conversión a MXN
            var precioMXN = precioUSD * tipoCambio;

            return (
                Math.Round(precioUSD, 2),
                Math.Round(precioMXN, 2)
            );
        }
    }
}
