using CotizacionMVC.Models.Enums;

namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Cliente
{
    public class ClienteDetalleDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? TelefonoMovil { get; set; }
        public string? Correo { get; set; }
        public string? NombreContacto { get; set; }
        public string? Calle { get; set; }
        public string? NumeroExterior { get; set; }
        public string? NumeroInterior { get; set; }
        public string? Colonia { get; set; }
        public string? Ciudad { get; set; }
        public string? Estado { get; set; }
        public string? CodigoPostal { get; set; }
        public string? Observaciones { get; set; }
        public string? Empresa { get; set; }
        public DateTime FechaRegistro { get; set; }
        public EstadoCliente EstadoCliente { get; set; }
        public DateTime FechaCreacion { get; set; }
        public List<CotizacionResumenDto> Cotizaciones { get; set; } = new();

    }
}
