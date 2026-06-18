using CotizacionMVC.Models.Enums;

namespace CotizacionMVC.Models.Entidades
{
    public class Empresa
    {
        private readonly List<Usuario> _usuariosAcceso = new List<Usuario>();
        private readonly List<Cotizacion> _cotizaciones = new List<Cotizacion>();
        private readonly List<Lead> _leads = new List<Lead>();

        public Guid Id { get; private set; }
        public string NombreComercial { get; private set; }
        public string? NombreLegal { get; private set; }  // ← Puede ser null
        public string Slug { get; private set; }
        public bool EsExclusivaTrane { get; private set; }
        public string MonedaBase { get; private set; }
        public decimal UtilidadEmpresaPorcentaje { get; private set; }
        public decimal UtilidadVendedorPorcentaje { get; private set; }
        public string? LogoUrl { get; private set; }  // ← Puede ser null hasta configurar
        public string? ColorPrimario { get; private set; }  // ← Puede ser null hasta configurar
        public string? ColorSecundario { get; private set; }  // ← Puede ser null hasta configurar
        public string? PlantillaPdfNombre { get; private set; }  // ← Puede ser null hasta configurar
        public string? TelefonoContacto { get; private set; }  // ← Puede ser null
        public string? CorreoContacto { get; private set; }  // ← Puede ser null
        public string? SitioWeb { get; private set; }  // ← Puede ser null
        public string? Eslogan { get; private set; }  // ← Puede ser null hasta configurar
        public bool Activa { get; private set; }
        public DateTime FechaCreacion { get; private set; }

        public IReadOnlyCollection<Usuario> UsuariosAcceso => _usuariosAcceso.AsReadOnly();
        public IReadOnlyCollection<Cotizacion> Cotizaciones => _cotizaciones.AsReadOnly();
        public IReadOnlyCollection<Lead> Leads => _leads.AsReadOnly();

        // Constructor protegido para EF Core
        protected Empresa()
        {
            NombreComercial = null!;
            Slug = null!;
            MonedaBase = null!;
            NombreLegal = null;
            LogoUrl = null;
            ColorPrimario = null;
            ColorSecundario = null;
            PlantillaPdfNombre = null;
            TelefonoContacto = null;
            CorreoContacto = null;
            SitioWeb = null;
            Eslogan = null;
        }

        public Empresa(
            string nombreComercial,
            string? nombreLegal,
            string slug,
            bool esExclusivaTrane,
            string monedaBase,
            decimal utilidadEmpresaPorcentaje,
            decimal utilidadVendedorPorcentaje,
            string? telefonoContacto,
            string? correoContacto)
        {
            if (string.IsNullOrWhiteSpace(nombreComercial))
                throw new ArgumentException("El nombre comercial es obligatorio");

            if (string.IsNullOrWhiteSpace(slug))
                throw new ArgumentException("El slug es obligatorio");

            if (monedaBase != "MXN" && monedaBase != "USD")
                throw new ArgumentException("La moneda base debe ser MXN o USD");

            if (utilidadEmpresaPorcentaje < 0)
                throw new ArgumentException("La utilidad de la empresa no puede ser negativa");

            if (utilidadVendedorPorcentaje < 0)
                throw new ArgumentException("La utilidad del vendedor no puede ser negativa");

            Id = Guid.NewGuid();
            NombreComercial = nombreComercial.Trim();
            NombreLegal = nombreLegal?.Trim();
            Slug = slug.Trim().ToLower();
            EsExclusivaTrane = esExclusivaTrane;
            MonedaBase = monedaBase;
            UtilidadEmpresaPorcentaje = utilidadEmpresaPorcentaje;
            UtilidadVendedorPorcentaje = utilidadVendedorPorcentaje;
            TelefonoContacto = telefonoContacto?.Trim();
            CorreoContacto = correoContacto?.Trim().ToLower();
            Activa = true;
            FechaCreacion = DateTime.UtcNow;

            // Inicializar propiedades visuales como null (se configurarán después)
            LogoUrl = null;
            ColorPrimario = null;
            ColorSecundario = null;
            PlantillaPdfNombre = null;
            SitioWeb = null;
            Eslogan = null;
        }

        public void ConfigurarIdentidadVisual(string logoUrl, string colorPrimario, string colorSecundario, string plantillaPdfNombre, string eslogan)
        {
            LogoUrl = logoUrl ?? throw new ArgumentNullException(nameof(logoUrl));
            ColorPrimario = colorPrimario ?? throw new ArgumentNullException(nameof(colorPrimario));
            ColorSecundario = colorSecundario ?? throw new ArgumentNullException(nameof(colorSecundario));
            PlantillaPdfNombre = plantillaPdfNombre ?? throw new ArgumentNullException(nameof(plantillaPdfNombre));
            Eslogan = eslogan ?? throw new ArgumentNullException(nameof(eslogan));
        }

        public void ConfigurarSitioWeb(string sitioWeb)
        {
            SitioWeb = sitioWeb?.Trim();
        }

        public void AgregarUsuarioAcceso(Usuario usuario)
        {
            if (usuario == null)
                throw new ArgumentNullException(nameof(usuario));

            if (!_usuariosAcceso.Any(u => u.Id == usuario.Id))
                _usuariosAcceso.Add(usuario);
        }

        public void Desactivar()
        {
            Activa = false;
        }

        public void Activar()
        {
            Activa = true;
        }

        public void ActualizarUtilidades(decimal utilidadEmpresa, decimal utilidadVendedor)
        {
            if (utilidadEmpresa < 0)
                throw new ArgumentException("La utilidad de la empresa no puede ser negativa");

            if (utilidadVendedor < 0)
                throw new ArgumentException("La utilidad del vendedor no puede ser negativa");

            UtilidadEmpresaPorcentaje = utilidadEmpresa;
            UtilidadVendedorPorcentaje = utilidadVendedor;
        }

        public void ActualizarContacto(string? telefono, string? correo)
        {
            if (!string.IsNullOrWhiteSpace(telefono))
                TelefonoContacto = telefono.Trim();

            if (!string.IsNullOrWhiteSpace(correo))
                CorreoContacto = correo.Trim().ToLower();
        }

        public bool TieneIdentidadVisualCompleta()
        {
            return !string.IsNullOrWhiteSpace(LogoUrl) &&
                   !string.IsNullOrWhiteSpace(ColorPrimario) &&
                   !string.IsNullOrWhiteSpace(ColorSecundario) &&
                   !string.IsNullOrWhiteSpace(PlantillaPdfNombre);
        }
    }
}