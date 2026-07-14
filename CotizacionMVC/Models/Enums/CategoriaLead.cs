namespace CotizacionMVC.Models.Enums
{
    public enum CategoriaLead
    {
        // --- Fase prospección ---
        SinContactar = 0,       // Recién creado, no se ha intentado contacto
        Frio = 1,               // Se intentó contacto, sin respuesta (SE CONSERVA)
        Contactado = 2,         // Hubo respuesta, está evaluando
        Caliente = 3,           // Mostró interés, pide información (SE CONSERVA)

        // --- Fase levantamiento ---
        Calificado = 4,         // Ya hay datos técnicos o visita realizada (SE CONSERVA)

        // --- Fase cotización ---
        Cotizando = 5,          // Ya tiene cotización enviada

        // --- Terminales ---
        NoInteresado = 10,      // Dijo que no
        Convertido = 20,        // Ya es cliente (cotización cerrada)
        Incontactable = 30      // Datos incorrectos, imposible contactar
    
    }
}
