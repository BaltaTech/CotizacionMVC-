namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Empresa
{
    public class ActualizarEmpresaDto
    {
        public Guid Id { get; set; }
        public decimal UtilidadEmpresaPorcentaje { get; set; }
        public decimal UtilidadVendedorPorcentaje { get; set; }
        public string? TelefonoContacto { get; set; }
        public string? CorreoContacto { get; set; }
        public string LogoUrl { get; set; } = string.Empty;
        public string ColorPrimario { get; set; } = string.Empty;
        public string ColorSecundario { get; set; } = string.Empty;
        public string PlantillaPdfNombre { get; set; } = string.Empty;
        public string Eslogan { get; set; } = string.Empty;
    }
}
