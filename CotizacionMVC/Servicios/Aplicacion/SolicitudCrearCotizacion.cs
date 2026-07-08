using CotizacionMVC.Controllers;
using CotizacionMVC.Models.Entidades;
using CotizacionMVC.ViewModels;

namespace CotizacionMVC.Servicios.Aplicacion
{
    public class SolicitudCrearCotizacion
    {
        public Guid ClienteId { get; set; }
        public Guid EmpresaId { get; set; }
        public Usuario Vendedor { get; set; } = null!;
        public decimal AreaMetrosCuadrados { get; set; }
        public string? CondicionesPago { get; set; }
        public List<ItemCotizacionJson> Equipos { get; set; } = new();
        public List<ItemInstalacionJson> Instalaciones { get; set; } = new();
    }

}
