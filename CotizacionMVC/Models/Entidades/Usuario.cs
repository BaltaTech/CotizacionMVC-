using Microsoft.AspNetCore.Identity;

namespace CotizacionMVC.Models.Entidades
{
    public class Usuario : IdentityUser<Guid>
    {
        // Propiedades personalizadas adicionales
        public string NombreCompleto { get; private set; }
        public bool Activo { get; private set; }
        public DateTime FechaRegistro { get; private set; }
        public DateTime? UltimoAcceso { get; private set; }

        // Relación con empresas (muchos a muchos)
        private readonly List<Empresa> _empresasAcceso = new();
        public IReadOnlyCollection<Empresa> EmpresasAcceso => _empresasAcceso.AsReadOnly();

        // Relación con cotizaciones
        private readonly List<Cotizacion> _cotizaciones = new();
        public IReadOnlyCollection<Cotizacion> Cotizaciones => _cotizaciones.AsReadOnly();

        // Constructor requerido por EF Core
        private Usuario() { }

        public Usuario(string nombreCompleto, string correoElectronico)
            : base(correoElectronico)  // UserName se establece con el correo
        {
            Id = Guid.NewGuid();
            NombreCompleto = nombreCompleto;
            Email = correoElectronico;
            Activo = true;
            FechaRegistro = DateTime.UtcNow;
        }

        // Métodos de dominio (se mantienen sin cambios)
        public void EstablecerContrasenia(string contraseniaHash)
        {
            PasswordHash = contraseniaHash;
        }

        public void RegistrarAcceso()
        {
            UltimoAcceso = DateTime.UtcNow;
        }

        public void Desactivar() => Activo = false;
        public void Activar() => Activo = true;
        public bool TieneContraseniaEstablecida() => !string.IsNullOrWhiteSpace(PasswordHash);

        public bool PuedeVerCotizacion(Cotizacion cotizacion)
        {
            // Nota: la comprobación de rol ahora usará Identity, pero podemos conservar la lógica
            if (cotizacion == null) return false;
            // El nombre del rol se manejará como string; podemos convertir el enum si es necesario,
            // pero es más sencillo consultar los roles de Identity directamente en el controlador.
            // Podemos mantener este método usando el Rol "Administrador" como string.
            // Más tarde lo adaptaremos.
            // Por ahora dejemos la lógica que ya tenías:
            return cotizacion.VendedorId == Id; // (ajustaremos cuando tengamos roles)
        }

        public bool PuedeVerEmpresa(Empresa empresa)
        {
            return _empresasAcceso.Any(e => e.Id == empresa.Id);
        }

        public void AgregarAccesoEmpresa(Empresa empresa)
        {
            if (!_empresasAcceso.Any(e => e.Id == empresa.Id))
                _empresasAcceso.Add(empresa);
        }

       
    }
}