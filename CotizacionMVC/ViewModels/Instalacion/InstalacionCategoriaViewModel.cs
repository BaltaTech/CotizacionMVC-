namespace CotizacionMVC.ViewModels.Instalacion
{
    public class InstalacionCategoriaViewModel
    {
        public string Categoria { get; set; } = string.Empty;
        public List<InstalacionItemViewModel> Items { get; set; } = new();
    }
}
