namespace CotizacionMVC.Models.Enums
{
    public enum ResultadoSeguimiento
    {
        SinRespuesta = 1,
        NoInteresado = 2,
        ReagendarLlamada = 3,
        SolicitoVisitaTecnica = 10,
        VisitaTecnicaRealizada = 11,
        DatosRecabados = 12,
        CotizacionSolicitada = 20,
        CotizacionEnviada = 21,
        NegociandoPrecio = 30,
        SolicitandoAlternativa = 31,
        EvaluandoFinanciamiento = 32,
        FechaTentativaInstalacion = 40,
        AnticipoSolicitado = 41,
        AnticipoRecibido = 42,
        Cerrada = 50,
        Perdida = 0
    }
}
