namespace CotizacionMVC.ViewModels
{
    public class ItemInstalacionJson
    {
        public Guid? InstalacionId { get; set; }
        public string Concepto { get; set; } = string.Empty;
        public string? Descripcion { get; set; }
        public int Cantidad { get; set; }
        public decimal CostoUnitario { get; set; }
    }
}
