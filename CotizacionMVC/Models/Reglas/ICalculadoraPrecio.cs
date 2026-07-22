namespace CotizacionMVC.Models.Reglas
{
    public interface ICalculadoraPrecio
    {       
        (decimal precioUSD, decimal precioMXN) Calcular(
            decimal precioBase,
            string monedaOriginal,
            decimal tipoCambio,
            params decimal[] factores
        );
    }   
}
