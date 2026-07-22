namespace CotizacionMVC.Models.Reglas
{
    public class CalculadoraPrecioEstandar : ICalculadoraPrecio
    {
        private const decimal UTILIDAD_DEFAULT_PORCENTAJE = 18m;

        public (decimal precioUSD, decimal precioMXN) Calcular(
            decimal precioBase,
            string monedaOriginal,
            decimal tipoCambio,
            params decimal[] factores)
        {
            // factores[0] = utilidad como porcentaje (ej: 18 = 18%)
            var utilidadPorcentaje = factores.Length > 0 && factores[0] > 0
                ? factores[0]
                : UTILIDAD_DEFAULT_PORCENTAJE;

            decimal precioMXN;

            if (monedaOriginal == "USD")
            {
                // Convertir a MXN primero
                var precioBaseMXN = precioBase * tipoCambio;
                // Aplicar margen de utilidad
                precioMXN = precioBaseMXN * (1 + utilidadPorcentaje / 100m);
            }
            else
            {
                // Ya está en MXN, aplicar margen directo
                precioMXN = precioBase * (1 + utilidadPorcentaje / 100m);
            }

            // Calcular equivalente USD para referencia
            var precioUSD = tipoCambio > 0
                ? precioMXN / tipoCambio
                : 0m;

            return (
                Math.Round(precioUSD, 2),
                Math.Round(precioMXN, 2)
            );
        }
    }
}
