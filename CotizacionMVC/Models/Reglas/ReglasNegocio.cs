namespace CotizacionMVC.Models.Reglas
{
    public static class ReglasNegocio
    {
        public const decimal IVA_PORCENTAJE = 0.16m;
        public const int DIAS_VALIDEZ_COTIZACION = 15;
        public const decimal MONTO_AUTORIZACION_DIRECCION_MXN = 500000m;
        public const decimal METROS_CUADRADOS_POR_TR = 16m;

        public static readonly string[] MARCAS_USD = { "TRANE", "YORK" };
        public static readonly string[] MONEDAS_VALIDAS = { "MXN", "USD" };

        public static int CalcularNumeroSecuencia(string prefijo, int ultimoNumero) => ultimoNumero + 1;

        public static string FormatearNumeroCotizacion(string prefijo, int numero) => $"{prefijo}-{numero:D3}";

        public static decimal CalcularCargaTermicaRecomendada(decimal metrosCuadrados, decimal factorAjuste = 1)
        {
            var trBase = metrosCuadrados / METROS_CUADRADOS_POR_TR;
            return Math.Round(trBase * factorAjuste, 1);
        }

        public static decimal ObtenerFactorAjustePorTipoEspacio(string tipoEspacio)
        {
            return tipoEspacio?.ToLower() switch
            {
                "residencial" => 1.0m,
                "oficina" => 1.1m,
                "comercial" => 1.2m,
                "restaurante" => 1.35m,
                "industrial" => 1.50m,
                _ => 1.0m
            };
        }

        // ========== CÁLCULO DE PRECIO POR MARCA ==========
        public static decimal CalcularPrecioUnitarioMxn(
            string marca,
            decimal precioCatalogo,
            string monedaOriginal,
            decimal tipoCambio,
            decimal factorA,    
            decimal factorB)    
        {
            decimal precioMxn;

            switch (marca.ToUpper())
            {
                case "TRANE":
                    // Precio USD × tipoCambio × (1 + factorA/100) × (1 + factorB/100)
                    var precioUsd = precioCatalogo * (1 + factorA / 100) * (1 + factorB / 100);
                    precioMxn = precioUsd * tipoCambio;
                    break;

                case "YORK":
                    // Precio USD × tipoCambio × (1 + factorA/100)
                    var precioYorkUsd = precioCatalogo * (1 + factorA / 100);
                    precioMxn = precioYorkUsd * tipoCambio;
                    break;

                default:
                    // TCL, HISENSE, etc. Precio MXN × (1 + factorA/100)
                    precioMxn = precioCatalogo * (1 + factorA / 100);
                    break;
            }

            return Math.Round(precioMxn, 2);
        }

       
        public static decimal ConvertirMxnAUsd(decimal montoMxn, decimal tipoCambio)
        {
            return tipoCambio > 0 ? Math.Round(montoMxn / tipoCambio, 2) : 0;
        }

       
        public static decimal ConvertirUsdAMxn(decimal montoUsd, decimal tipoCambio)
        {
            return Math.Round(montoUsd * tipoCambio, 2);
        }
        public static decimal CalcularRecargoCiudad(decimal subtotalEquipos, decimal porcentajeCiudad)
        {
            return Math.Round(subtotalEquipos * porcentajeCiudad / 100, 2);
        }

        public static decimal CalcularIva(decimal subtotal)
        {
            return Math.Round(subtotal * IVA_PORCENTAJE, 2);
        }
        public static decimal CalcularTotal(decimal subtotal)
        {
            return Math.Round(subtotal + CalcularIva(subtotal), 2);
        }
    }
}