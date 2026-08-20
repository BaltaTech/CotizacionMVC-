namespace CotizacionMVC.Models.Reglas
{
    public static class ReglasNegocio
    {        
          

        public static ICalculadoraPrecio ObtenerCalculadora(string marca)
        {
            return marca?.ToUpper() switch
            {
                "TRANE" => new CalculadoraPrecioTrane(),
                "HISENSE" => new CalculadoraPrecioHisense(),
                "TCL" => new CalculadoraPrecioTCL(),
                _ => new CalculadoraPrecioEstandar()
            };
        }

    }
}