using CotizacionMVC.Models.Enums;

namespace CotizacionMVC.Models.Entidades
{
    public class Usuario
    {
        private readonly List<Empresa> _empresasAcceso = new List<Empresa>();
        private readonly List<Cotizacion> _cotizaciones = new List<Cotizacion>();
        private readonly List<Seguimiento> _seguimientosCreados = new List<Seguimiento>();
        private readonly List<Lead> _leadsAsignados = new List<Lead>();

        public Guid Id { get; private set; }
        public string NombreCompleto { get; private set; }
        public string CorreoElectronico { get; private set; }
        public string? ContraseniaHash { get; private set; }  // ← Puede ser null hasta establecerla
        public RolUsuario Rol { get; private set; }
        public bool Activo { get; private set; }
        public DateTime FechaRegistro { get; private set; }
        public DateTime? UltimoAcceso { get; private set; }

        public IReadOnlyCollection<Empresa> EmpresasAcceso => _empresasAcceso.AsReadOnly();
        public IReadOnlyCollection<Cotizacion> Cotizaciones => _cotizaciones.AsReadOnly();

        // Constructor protegido para EF Core
        protected Usuario()
        {
            NombreCompleto = null!;
            CorreoElectronico = null!;
            ContraseniaHash = null;
        }

        public Usuario(string nombreCompleto, string correoElectronico, RolUsuario rol)
        {
            if (string.IsNullOrWhiteSpace(nombreCompleto))
                throw new ArgumentException("El nombre completo es obligatorio");

            if (string.IsNullOrWhiteSpace(correoElectronico))
                throw new ArgumentException("El correo electrónico es obligatorio");

            if (!correoElectronico.Contains("@"))
                throw new ArgumentException("El correo electrónico no es válido");

            Id = Guid.NewGuid();
            NombreCompleto = nombreCompleto.Trim();
            CorreoElectronico = correoElectronico.Trim().ToLower();
            Rol = rol;
            Activo = true;
            FechaRegistro = DateTime.UtcNow;
            ContraseniaHash = null;
        }

        public void EstablecerContrasenia(string contraseniaHash)
        {
            if (string.IsNullOrWhiteSpace(contraseniaHash))
                throw new ArgumentException("La contraseña es obligatoria");

            ContraseniaHash = contraseniaHash;
        }

        public void RegistrarAcceso()
        {
            UltimoAcceso = DateTime.UtcNow;
        }

        public void Desactivar()
        {
            Activo = false;
        }

        public void Activar()
        {
            Activo = true;
        }

        public bool TieneContraseniaEstablecida()
        {
            return !string.IsNullOrWhiteSpace(ContraseniaHash);
        }

        public bool PuedeVerCotizacion(Cotizacion cotizacion)
        {
            if (cotizacion == null)
                return false;

            if (Rol == RolUsuario.Administrador)
                return true;

            return cotizacion.VendedorId == Id;
        }

        public bool PuedeVerEmpresa(Empresa empresa)
        {
            if (empresa == null)
                return false;

            if (Rol == RolUsuario.Administrador)
                return true;

            return _empresasAcceso.Any(e => e.Id == empresa.Id);
        }

        public void AgregarAccesoEmpresa(Empresa empresa)
        {
            if (empresa == null)
                throw new ArgumentNullException(nameof(empresa));

            if (!_empresasAcceso.Any(e => e.Id == empresa.Id))
                _empresasAcceso.Add(empresa);
        }

        // Método para obtener las empresas a las que tiene acceso (conveniencia)
        public IReadOnlyCollection<Empresa> ObtenerEmpresasAcceso()
        {
            return _empresasAcceso.AsReadOnly();
        }

        // Método para verificar si el usuario es administrador
        public bool EsAdministrador()
        {
            return Rol == RolUsuario.Administrador;
        }

        // Método para verificar si el usuario es vendedor
        public bool EsVendedor()
        {
            return Rol == RolUsuario.Vendedor;
        }
    }
}