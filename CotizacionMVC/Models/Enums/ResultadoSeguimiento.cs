namespace CotizacionMVC.Models.Enums
{
    public enum ResultadoSeguimiento
    {
        // --- Fase prospección / contacto inicial ---
        SinRespuesta = 1,
        NoInteresado = 2,
        ReagendarLlamada = 3,

        // --- Fase levantamiento técnico ---
        SolicitoVisitaTecnica = 10,
        VisitaTecnicaRealizada = 11,
        DatosRecabados = 12,

        // --- Fase cotización ---
        CotizacionSolicitada = 20,
        CotizacionEnviada = 21,

        // --- Fase negociación ---
        NegociandoPrecio = 30,
        SolicitandoAlternativa = 31,
        EvaluandoFinanciamiento = 32,

        // --- Fase cierre ---
        FechaTentativaInstalacion = 40,
        AnticipoSolicitado = 41,
        AnticipoRecibido = 42,
        Cerrada = 50,
        Perdida = 0
    }
}
