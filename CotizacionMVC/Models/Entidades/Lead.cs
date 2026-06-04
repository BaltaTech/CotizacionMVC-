using CotizacionMVC.Models.Enums;

namespace CotizacionMVC.Models.Entidades
{
    public class Lead
    {
        public Guid Id { get; private set; }
        public Guid EmpresaId { get; private set; }
        public virtual Empresa Empresa { get; private set; }
        public Guid? VendedorAsignadoId { get; private set; }
        public virtual Usuario? VendedorAsignado { get; private set; }  // ← Puede ser null hasta asignar
        public string NombreContacto { get; private set; }
        public string? Telefono { get; private set; }  // ← Puede ser null
        public string? CorreoElectronico { get; private set; }  // ← Puede ser null
        public string? EmpresaCliente { get; private set; }  // ← Puede ser null
        public CategoriaLead Categoria { get; private set; }
        public string Origen { get; private set; }
        public DateTime FechaCreacion { get; private set; }
        public DateTime? FechaAsignacion { get; private set; }
        public string? ComentariosInternos { get; private set; }  // ← Puede ser null

        // Constructor protegido para EF Core
        protected Lead()
        {
            Empresa = null!;
            NombreContacto = null!;
            Origen = null!;
            Telefono = null;
            CorreoElectronico = null;
            EmpresaCliente = null;
            ComentariosInternos = null;
            VendedorAsignado = null;
        }

        public Lead(
            Empresa empresa,
            string nombreContacto,
            string? telefono,
            CategoriaLead categoria,
            string origen,
            string? correoElectronico = null)
        {
            if (empresa == null)
                throw new ArgumentNullException(nameof(empresa));

            if (string.IsNullOrWhiteSpace(nombreContacto))
                throw new ArgumentException("El nombre de contacto es obligatorio");

            bool tieneTelefono = !string.IsNullOrWhiteSpace(telefono);
            bool tieneCorreo = !string.IsNullOrWhiteSpace(correoElectronico);

            if (!tieneTelefono && !tieneCorreo)
                throw new ArgumentException("Debe proporcionar al menos teléfono o correo electrónico");

            if (string.IsNullOrWhiteSpace(origen))
                throw new ArgumentException("El origen del lead es obligatorio");

            Id = Guid.NewGuid();
            Empresa = empresa;
            EmpresaId = empresa.Id;
            NombreContacto = nombreContacto.Trim();
            Telefono = telefono?.Trim();
            CorreoElectronico = correoElectronico?.Trim().ToLower();
            Categoria = categoria;
            Origen = origen.Trim();
            FechaCreacion = DateTime.UtcNow;
            EmpresaCliente = null;
            ComentariosInternos = null;
            VendedorAsignado = null;
        }

        public void AsignarVendedor(Usuario vendedor)
        {
            if (vendedor == null)
                throw new ArgumentNullException(nameof(vendedor));

            if (vendedor.Rol != RolUsuario.Vendedor)
                throw new InvalidOperationException("Solo se pueden asignar vendedores a un lead");

            if (VendedorAsignadoId.HasValue)
                throw new InvalidOperationException("Este lead ya tiene un vendedor asignado");

            VendedorAsignado = vendedor;
            VendedorAsignadoId = vendedor.Id;
            FechaAsignacion = DateTime.UtcNow;
        }

        public void ActualizarCategoria(CategoriaLead nuevaCategoria)
        {
            Categoria = nuevaCategoria;
        }

        public void AgregarComentario(string? comentario)
        {
            if (!string.IsNullOrWhiteSpace(comentario))
                ComentariosInternos = comentario.Trim();
        }

        public void ActualizarDatosContacto(string? telefono, string? correo, string? empresaCliente)
        {
            if (!string.IsNullOrWhiteSpace(telefono))
                Telefono = telefono.Trim();

            if (!string.IsNullOrWhiteSpace(correo))
                CorreoElectronico = correo.Trim().ToLower();

            if (!string.IsNullOrWhiteSpace(empresaCliente))
                EmpresaCliente = empresaCliente.Trim();
        }

        // Método para verificar si el lead tiene vendedor asignado
        public bool TieneVendedorAsignado()
        {
            return VendedorAsignadoId.HasValue && VendedorAsignado != null;
        }

        // Método para obtener el medio de contacto principal
        public string ObtenerMedioContactoPrincipal()
        {
            if (!string.IsNullOrWhiteSpace(Telefono))
                return $"Tel: {Telefono}";
            if (!string.IsNullOrWhiteSpace(CorreoElectronico))
                return $"Email: {CorreoElectronico}";
            return "Sin medio de contacto";
        }

        // Método para saber si el lead está caliente (alta probabilidad)
        public bool EsLeadCaliente()
        {
            return Categoria == CategoriaLead.Caliente || Categoria == CategoriaLead.Calificado;
        }
    }
}