namespace CotizacionMVC.Models.Reglas
{
    public class CalculadoraPrecioHisense : ICalculadoraPrecio
    {
        public (decimal precioUSD, decimal precioMXN) Calcular(
            decimal precioBase,
            string monedaOriginal,
            decimal tipoCambio,
            params decimal[] factores)
        {
            var precioMXN = precioBase;
            var precioUSD = tipoCambio > 0
                ? Math.Round(precioMXN / tipoCambio, 2)
                : 0;

            return (precioUSD, precioMXN);
        }
    }
}
