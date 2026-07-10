namespace CotizacionMVC.Servicios.Aplicacion.Dtos.Empresa
{
    public class EmpresaDetalleDto
    {
        public Guid Id { get; set; }
        public string NombreComercial { get; set; } = string.Empty;
        public string? NombreLegal { get; set; }
        public string Slug { get; set; } = string.Empty;
        public bool EsExclusivaTrane { get; set; }
        public string MonedaBase { get; set; } = string.Empty;
        public decimal UtilidadEmpresaPorcentaje { get; set; }
        public decimal UtilidadVendedorPorcentaje { get; set; }
        public string? LogoUrl { get; set; }
        public string? ColorPrimario { get; set; }
        public string? ColorSecundario { get; set; }
        public string? PlantillaPdfNombre { get; set; }
        public string? TelefonoContacto { get; set; }
        public string? CorreoContacto { get; set; }
        public string? SitioWeb { get; set; }
        public string? Eslogan { get; set; }
        public bool Activa { get; set; }
        public DateTime FechaCreacion { get; set; }
    }
}
