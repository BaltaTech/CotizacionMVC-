namespace CotizacionMVC.Models.Enums
{
    public enum EstadoCotizacion
    {
        InformacionSolicitada = 10,
        CotizacionEnviada = 20,
        PreguntaInstalacion = 30,
        NegociandoPrecio = 50,
        FechaTentativa = 70,
        Aceptada = 90,
        PagoAnticipo = 95,
        Cerrada = 100,
        Perdida = 0
    }
}
