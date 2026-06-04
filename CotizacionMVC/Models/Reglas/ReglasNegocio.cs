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

        public static int CalcularNumeroSecuencia(string prefijo, int ultimoNumero)
        {
            return ultimoNumero + 1;
        }

        public static string FormatearNumeroCotizacion(string prefijo, int numero)
        {
            return $"{prefijo}-{numero:D3}";
        }

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
    }
}
