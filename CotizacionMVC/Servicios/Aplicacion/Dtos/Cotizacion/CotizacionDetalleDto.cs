using CotizacionMVC.Models.Enums;

namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Cotizacion
{
    public class CotizacionDetalleDto
    {
        public Guid Id { get; set; }
        public string NumeroCotizacion { get; set; } = string.Empty;
        public Guid ClienteId { get; set; }
        public string ClienteNombre { get; set; } = string.Empty;
        public Guid EmpresaId { get; set; }
        public string EmpresaNombre { get; set; } = string.Empty;
        public Guid VendedorId { get; set; }
        public string VendedorNombre { get; set; } = string.Empty;
        public DateTime FechaCreacion { get; set; }
        public DateTime FechaVencimiento { get; set; }
        public EstadoCotizacion Estado { get; set; }
        public decimal AreaMetrosCuadrados { get; set; }
        public string CondicionesPago { get; set; } = string.Empty;
        public decimal Subtotal { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }
        public string Moneda { get; set; } = string.Empty;
        public bool PuedeSerModificada { get; set; }
        public List<ItemCotizacionDto> Equipos { get; set; } = new();
        public List<ItemInstalacionDto> Instalaciones { get; set; } = new();
    }

    public class ItemCotizacionDto
    {
        public string EquipoMarca { get; set; } = string.Empty;
        public string EquipoModelo { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal PrecioUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }

    public class ItemInstalacionDto
    {
        public string Concepto { get; set; } = string.Empty;
        public string Descripcion { get; set; } = string.Empty;
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
        public decimal Subtotal { get; set; }
    }
}