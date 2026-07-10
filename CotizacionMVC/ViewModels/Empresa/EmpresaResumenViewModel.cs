namespace CotizacionMVC.ViewModels.Empresa
{
    public class EmpresaResumenViewModel
    {
        public Guid Id { get; set; }
        public string NombreComercial { get; set; } = string.Empty;
        public string Slug { get; set; } = string.Empty;
        public bool Activa { get; set; }
        public string? LogoUrl { get; set; }
        public string? ColorPrimario { get; set; }
    }
}
