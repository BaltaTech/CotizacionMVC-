namespace CotizacionMVC.Servicios.Aplicacion.Dtos.AdminDashboard
{
    public class PipelineEtapaDto
    {
        public string Etapa { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal MontoEstimado { get; set; }
    }
}