namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Seguimientos
{
    public class ResultadoCrearLead
    {
        public bool Exitoso { get; set; }
        public string? MensajeError { get; set; }
        public Guid? LeadId { get; set; }

        public static ResultadoCrearLead Exito(Guid leadId)
        {
            return new ResultadoCrearLead { Exitoso = true, LeadId = leadId };
        }

        public static ResultadoCrearLead Error(string mensaje)
        {
            return new ResultadoCrearLead { Exitoso = false, MensajeError = mensaje };
        }
    }
}