using CotizacionMVC.Models.Enums;

namespace CotizacionMVC.Models.Entidades
{
    public class Seguimiento
    {
        public Guid Id { get; private set; }
        public Guid CotizacionId { get; private set; }
        public virtual Cotizacion Cotizacion { get; private set; }
        public Guid EmpresaId { get; private set; }
        public virtual Empresa Empresa { get; private set; }
        public Guid VendedorId { get; private set; }
        public virtual Usuario Vendedor { get; private set; }
        public DateTime FechaProgramada { get; private set; }
        public DateTime FechaCreacion { get; private set; }
        public DateTime? FechaCompletado { get; private set; }
        public EstadoSeguimiento Estado { get; private set; }
        public string? Comentarios { get; private set; }  // ← Puede ser null
        public MedioContacto MedioContacto { get; private set; }

        // Constructor protegido para EF Core
        protected Seguimiento()
        {
            Cotizacion = null!;
            Empresa = null!;
            Vendedor = null!;
            Comentarios = null;
        }

        public Seguimiento(
            Cotizacion cotizacion,
            Usuario vendedor,
            DateTime fechaProgramada,
            MedioContacto medioContacto,
            string? comentarios = null)
        {
            if (cotizacion == null)
                throw new ArgumentNullException(nameof(cotizacion));

            if (vendedor == null)
                throw new ArgumentNullException(nameof(vendedor));

            if (fechaProgramada.Date < DateTime.UtcNow.Date)
                throw new ArgumentException("La fecha programada no puede ser anterior a hoy");

            Id = Guid.NewGuid();
            Cotizacion = cotizacion;
            CotizacionId = cotizacion.Id;
            EmpresaId = cotizacion.EmpresaId;
            Empresa = cotizacion.Empresa;
            Vendedor = vendedor;
            VendedorId = vendedor.Id;
            FechaProgramada = fechaProgramada.Date;
            FechaCreacion = DateTime.UtcNow;
            Estado = EstadoSeguimiento.Pendiente;
            MedioContacto = medioContacto;
            Comentarios = comentarios?.Trim();
        }

        public void Completar(string? comentarios = null)
        {
            if (Estado == EstadoSeguimiento.Completado)
                throw new InvalidOperationException("Este seguimiento ya fue completado");

            Estado = EstadoSeguimiento.Completado;
            FechaCompletado = DateTime.UtcNow;

            if (!string.IsNullOrWhiteSpace(comentarios))
                Comentarios = comentarios.Trim();
        }

        public bool EstaVencido()
        {
            return Estado == EstadoSeguimiento.Pendiente &&
                   FechaProgramada.Date <= DateTime.UtcNow.Date;
        }

        public bool EsRecordatorio()
        {
            return Estado == EstadoSeguimiento.Pendiente &&
                   FechaProgramada.Date <= DateTime.UtcNow.Date;
        }

        // Método para actualizar la fecha programada (útil para reprogramar)
        public void Reprogramar(DateTime nuevaFecha)
        {
            if (Estado == EstadoSeguimiento.Completado)
                throw new InvalidOperationException("No se puede reprogramar un seguimiento ya completado");

            if (nuevaFecha.Date < DateTime.UtcNow.Date)
                throw new ArgumentException("La nueva fecha no puede ser anterior a hoy");

            FechaProgramada = nuevaFecha.Date;
        }

        // Método para agregar o actualizar comentarios
        public void ActualizarComentarios(string comentarios)
        {
            Comentarios = comentarios?.Trim();
        }
    }
}