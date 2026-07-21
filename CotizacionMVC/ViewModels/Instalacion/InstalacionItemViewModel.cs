namespace CotizacionMVC.ViewModels.Instalacion
{
    public class InstalacionItemViewModel
    {
        public Guid Id { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public decimal CostoUnitario { get; set; }
    }
}
