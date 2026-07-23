namespace CotizacionMVC.Models.Enums
{
    public enum EtapaNegociacion
    {
        SinContactar = 0,
        ContactoInicial = 5,
        InformacionSolicitada = 10,
        CotizacionEnviada = 20,
        PreguntaInstalacion = 30,
        NegociandoPrecio = 50,
        FechaTentativa = 70,
        CotizacionFirmada = 90,
        AnticipoRecibido = 100,
        Cerrada = 110,
        Perdida = -1
    }
}
