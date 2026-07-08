using CotizacionMVC.Models.Enums;
using CotizacionMVC.Models.Valor;

namespace CotizacionMVC.Models.Entidades
{
    public class Cliente
    {
        private readonly List<Cotizacion> _cotizaciones = new List<Cotizacion>();
        public Guid Id { get; private set; }
        public string Nombre { get; private set; }
        public Contacto Contacto { get; private set; }
        public Direccion? Direccion { get; private set; }  
        public DateTime FechaRegistro { get; private set; }
        public string? Observaciones { get; private set; }
        public IReadOnlyCollection<Cotizacion> Cotizaciones => _cotizaciones.AsReadOnly();
        // Agregar usando: using CotizacionMVC.Models.Enums;
        public OrigenCliente Origen { get; private set; }
        public Guid? VendedorAsignadoId { get; private set; }
        public Guid RegistradoPorId { get; private set; }
        public DateTime? FechaAsignacion { get; private set; }
        public DateTime? FechaCotizacion { get; private set; }
        public MotivoNoCotizable? MotivoNoCotizable { get; private set; }
        public string? ComentarioNoCotizable { get; private set; }
        public EstadoCliente Estado { get; private set; }
        public string Folio { get; private set; }



        // Constructor protegido para EF Core
        protected Cliente()
        {
            Nombre = null!;
            Contacto = null!;
            Direccion = null;
            Observaciones = null;
            Folio = null!;
        }
        public Cliente(string nombre, Contacto contacto)
        {
            if (string.IsNullOrWhiteSpace(nombre))
                throw new ArgumentException("El nombre del cliente es obligatorio");

            if (contacto == null)
                throw new ArgumentNullException(nameof(contacto));

            Id = Guid.NewGuid();
            Nombre = nombre.Trim();
            Contacto = contacto;
            FechaRegistro = DateTime.UtcNow;
            Direccion = null;
            Observaciones = null;
        }
        public void ActualizarContacto(Contacto nuevoContacto)
        {
            if (nuevoContacto == null)
                throw new ArgumentNullException(nameof(nuevoContacto));

            Contacto = nuevoContacto;
        }
        public void ActualizarDireccion(Direccion? direccion)
        {
            Direccion = direccion;
        }
        public void AgregarObservaciones(string? observaciones)
        {
            if (!string.IsNullOrWhiteSpace(observaciones))
                Observaciones = observaciones.Trim();
        }
        // Método para verificar si el cliente tiene dirección registrada
        public bool TieneDireccion()
        {
            return Direccion != null &&
                       !string.IsNullOrWhiteSpace(Direccion.CodigoPostal);
        }
        public string ObtenerDireccionMostrable()
        {
            if (TieneDireccion())
                return Direccion!.DireccionCompleta();

            return "Sin dirección registrada";
        }
        public string ObtenerContactoPrincipal()
        {
            return Contacto.ObtenerMedioPrincipal();
        }
        public bool TieneContacto()
        {
            return Contacto.TieneMedioDeContacto();
        }

        public void AsignarVendedor(Guid vendedorId)
        {
            VendedorAsignadoId = vendedorId;
            Estado = EstadoCliente.Asignado;
            FechaAsignacion = DateTime.UtcNow;
        }

        public void Contactar()
        {
            Estado = EstadoCliente.Contactado;
        }

        public void MarcarCotizado()
        {
            Estado = EstadoCliente.Cotizado;
            FechaCotizacion = DateTime.UtcNow;
        }

        public void MarcarNoCotizable(MotivoNoCotizable motivo, string? comentario = null)
        {
            Estado = EstadoCliente.NoCotizable;
            MotivoNoCotizable = motivo;
            ComentarioNoCotizable = comentario?.Trim();
        }

        public void ReasignarVendedor(Guid nuevoVendedorId)
        {
            VendedorAsignadoId = nuevoVendedorId;
            FechaAsignacion = DateTime.UtcNow;
        }

        public void ConfigurarRegistro(OrigenCliente origen, Guid registradoPorId)
        {
            if (registradoPorId == Guid.Empty)
                throw new ArgumentException("El ID de quien registra es obligatorio");

            Origen = origen;
            RegistradoPorId = registradoPorId;

            // Si ya tiene vendedor, mantener el estado Asignado
            // Si no tiene vendedor, dejarlo como SinAsignar
            if (VendedorAsignadoId.HasValue)
                Estado = EstadoCliente.Asignado;
            else
                Estado = EstadoCliente.SinAsignar;
        }

        public void MarcarPendienteAsignar()
        {
            Estado = EstadoCliente.SinAsignar;
        }

        public void AsignarFolio(string folio)
        {
            if (string.IsNullOrWhiteSpace(folio))
                throw new ArgumentException("El folio es obligatorio");

            Folio = folio;
        }
    }
}