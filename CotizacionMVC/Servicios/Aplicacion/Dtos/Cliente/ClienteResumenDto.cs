using CotizacionMVC.Models.Enums;

namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Cliente
{
    public class ClienteResumenDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Correo { get; set; }
        public EstadoCliente Estado { get; set; }
        public int CantidadCotizaciones { get; set; }
        public string? Empresa { get; set; }
        public DateTime FechaRegistro { get; set; }
    }
}
