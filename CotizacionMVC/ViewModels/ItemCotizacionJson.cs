namespace CotizacionMVC.ViewModels
{
    public class ItemCotizacionJson
    {
        public Guid EquipoId { get; set; }
        public int Cantidad { get; set; }
        public decimal FactorPrecio { get; set; }
        public decimal FactorUtilidad { get; set; }
        public string Marca { get; set; } = string.Empty;
    }
}