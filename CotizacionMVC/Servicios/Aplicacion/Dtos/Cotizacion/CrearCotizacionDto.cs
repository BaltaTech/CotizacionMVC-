using CotizacionMVC.ViewModels;

namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Cotizacion
{
    public class CrearCotizacionDto
    {
        public Guid ClienteId { get; set; }
        public Guid EmpresaId { get; set; }
        public Guid VendedorId { get; set; }
        public decimal AreaMetrosCuadrados { get; set; }
        public string CondicionesPago { get; set; } = string.Empty;
        public List<ItemCotizacionJson> Equipos { get; set; } = new();
        public List<ItemInstalacionJson> Instalaciones { get; set; } = new();
        public Guid? LeadId { get; set; }
        public decimal TipoCambio { get; set; } = 17.43m;
        public decimal UtilidadPorcentaje { get; set; } = 18m;
        public decimal RecargoCiudadPorcentaje { get; set; } = 0;
    }
}
