using System.ComponentModel.DataAnnotations;

namespace CotizacionMVC.ViewModels.Empresa
{
    public class EmpresaFormViewModel
    {
        public Guid Id { get; set; }

        [Required(ErrorMessage = "El nombre comercial es obligatorio")]
        public string NombreComercial { get; set; } = string.Empty;

        [Range(0, 100, ErrorMessage = "La utilidad de la empresa debe estar entre 0 y 100")]
        public decimal UtilidadEmpresaPorcentaje { get; set; }

        [Range(0, 100, ErrorMessage = "La utilidad del vendedor debe estar entre 0 y 100")]
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
