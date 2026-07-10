namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Cliente
{
    public class CotizacionResumenDto
    {
        public string NumeroCotizacion { get; set; } = string.Empty;
        public string? EmpresaNombre { get; set; }
        public DateTime FechaCreacion { get; set; }
        public string Total { get; set; } = string.Empty;
        public string Estado { get; set; } = string.Empty;
    }
}
