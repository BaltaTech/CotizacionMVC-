namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Cotizacion
{
    public class DatosCrearCotizacionDto
    {
        public List<ClienteResumenDto> Clientes { get; set; } = new();
        public List<EquipoResumenDto> Equipos { get; set; } = new();
        public List<InstalacionResumenDto> Instalaciones { get; set; } = new();
        public LeadResumenDto? Lead { get; set; }
    }

    public class ClienteResumenDto
    {
        public Guid Id { get; set; }
        public string Nombre { get; set; } = string.Empty;
        public string? Telefono { get; set; }
        public string? Folio { get; set; }
        public int Estado { get; set; }
        public DateTime FechaRegistro { get; set; }
        public string? Observaciones { get; set; }
        public bool TieneVendedor { get; set; }
    }

    public class EquipoResumenDto
    {
        public Guid Id { get; set; }
        public string Marca { get; set; } = string.Empty;
        public string Modelo { get; set; } = string.Empty;
        public decimal CapacidadToneladas { get; set; }
        public decimal PrecioBase { get; set; }
        public string MonedaOriginal { get; set; } = string.Empty;
    }

    public class InstalacionResumenDto
    {
        public Guid Id { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal CostoUnitario { get; set; }
    }
}