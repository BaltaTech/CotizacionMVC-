using CotizacionMVC.Models.Enums;

namespace CotizacionMVC.Models.Entidades
{
    public class Seguimiento
    {
        public Guid Id { get; private set; }
        public Guid? LeadId { get; private set; }
        public virtual Lead? Lead { get; private set; }
        public Guid? CotizacionId { get; private set; }
        public virtual Cotizacion? Cotizacion { get; private set; }
        public Guid EmpresaId { get; private set; }
        public virtual Empresa Empresa { get; private set; }
        public Guid VendedorId { get; private set; }
        public virtual Usuario Vendedor { get; private set; }
 
        public DateTime FechaCreacion { get; private set; }
        public DateTime FechaContacto { get; private set; }
        public MedioContacto MedioContacto { get; private set; }
        public ResultadoSeguimiento Resultado { get; private set; }
        public string? Notas { get; private set; }
        public DateTime? ProximoContacto { get; private set; }
        public bool RecordatorioEnviado { get; private set; }

         protected Seguimiento()
        {
            Empresa = null!;
            Vendedor = null!;
            Notas = null;
        }

         public Seguimiento(
            Lead lead,
            Usuario vendedor,
            DateTime fechaContacto,
            MedioContacto medioContacto,
            ResultadoSeguimiento resultado,
            string? notas = null,
            DateTime? proximoContacto = null)
        {
            if (lead == null)
                throw new ArgumentNullException(nameof(lead));

            ValidarVendedor(vendedor, lead.Empresa);
            ValidarFechaContacto(fechaContacto);
            ValidarProximoContacto(fechaContacto, proximoContacto);

            Id = Guid.NewGuid();
            Lead = lead;
            LeadId = lead.Id;
            Cotizacion = null;
            CotizacionId = null;
            Empresa = lead.Empresa;
            EmpresaId = lead.EmpresaId;
            Vendedor = vendedor;
            VendedorId = vendedor.Id;
            FechaCreacion = DateTime.UtcNow;
            FechaContacto = fechaContacto;
            MedioContacto = medioContacto;
            Resultado = resultado;
            Notas = notas?.Trim();
            ProximoContacto = proximoContacto;
            RecordatorioEnviado = false;
        }

        // Constructor para seguimiento de COTIZACION
        public Seguimiento(
            Cotizacion cotizacion,
            Usuario vendedor,
            DateTime fechaContacto,
            MedioContacto medioContacto,
            ResultadoSeguimiento resultado,
            string? notas = null,
            DateTime? proximoContacto = null)
        {
            if (cotizacion == null)
                throw new ArgumentNullException(nameof(cotizacion));

            ValidarVendedor(vendedor, cotizacion.Empresa);
            ValidarFechaContacto(fechaContacto);
            ValidarProximoContacto(fechaContacto, proximoContacto);

            Id = Guid.NewGuid();
            Lead = null;
            LeadId = null;
            Cotizacion = cotizacion;
            CotizacionId = cotizacion.Id;
            Empresa = cotizacion.Empresa;
            EmpresaId = cotizacion.EmpresaId;
            Vendedor = vendedor;
            VendedorId = vendedor.Id;
            FechaCreacion = DateTime.UtcNow;
            FechaContacto = fechaContacto;
            MedioContacto = medioContacto;
            Resultado = resultado;
            Notas = notas?.Trim();
            ProximoContacto = proximoContacto;
            RecordatorioEnviado = false;
        }

        // Constructor para seguimiento de LEAD + COTIZACION  
        public Seguimiento(
            Lead lead,
            Cotizacion cotizacion,
            Usuario vendedor,
            DateTime fechaContacto,
            MedioContacto medioContacto,
            ResultadoSeguimiento resultado,
            string? notas = null,
            DateTime? proximoContacto = null)
        {
            if (lead == null)
                throw new ArgumentNullException(nameof(lead));

            if (cotizacion == null)
                throw new ArgumentNullException(nameof(cotizacion));

            ValidarVendedor(vendedor, lead.Empresa);
            ValidarFechaContacto(fechaContacto);
            ValidarProximoContacto(fechaContacto, proximoContacto);

            Id = Guid.NewGuid();
            Lead = lead;
            LeadId = lead.Id;
            Cotizacion = cotizacion;
            CotizacionId = cotizacion.Id;
            Empresa = lead.Empresa;
            EmpresaId = lead.EmpresaId;
            Vendedor = vendedor;
            VendedorId = vendedor.Id;
            FechaCreacion = DateTime.UtcNow;
            FechaContacto = fechaContacto;
            MedioContacto = medioContacto;
            Resultado = resultado;
            Notas = notas?.Trim();
            ProximoContacto = proximoContacto;
            RecordatorioEnviado = false;
        }

        public void MarcarRecordatorioEnviado()
        {
            RecordatorioEnviado = true;
        }

        public void ActualizarNotas(string notas)
        {
            Notas = notas?.Trim();
        }

        public void ReprogramarProximoContacto(DateTime nuevaFecha)
        {
            if (nuevaFecha <= DateTime.UtcNow)
                throw new ArgumentException("El próximo contacto debe ser una fecha futura");

            ProximoContacto = nuevaFecha;
            RecordatorioEnviado = false;
        }

        public bool TieneProximoContactoPendiente()
        {
            return ProximoContacto.HasValue
                && ProximoContacto.Value.Date >= DateTime.UtcNow.Date
                && !RecordatorioEnviado;
        }

        public bool ProximoContactoEsHoy()
        {
            return ProximoContacto.HasValue
                && ProximoContacto.Value.Date == DateTime.UtcNow.Date;
        }

        public bool EstaVencido()
        {
            return ProximoContacto.HasValue
                && ProximoContacto.Value.Date < DateTime.UtcNow.Date
                && !RecordatorioEnviado;
        }

        public bool EsDeVendedor(Guid vendedorId)
        {
            return VendedorId == vendedorId;
        }

        public bool PerteneceAEmpresa(Guid empresaId)
        {
            return EmpresaId == empresaId;
        }

        public bool EsDeLead()
        {
            return LeadId.HasValue;
        }

        public bool EsDeCotizacion()
        {
            return CotizacionId.HasValue;
        }

    
        private void ValidarVendedor(Usuario vendedor, Empresa empresa)
        {
            if (vendedor == null)
                throw new ArgumentNullException(nameof(vendedor));
        }

        private void ValidarFechaContacto(DateTime fechaContacto)
        {
            if (fechaContacto > DateTime.UtcNow.AddMinutes(5))
                throw new ArgumentException("La fecha de contacto no puede ser futura (máximo 5 minutos de margen)");
        }

        private void ValidarProximoContacto(DateTime fechaContacto, DateTime? proximoContacto)
        {
            if (proximoContacto.HasValue && proximoContacto.Value <= fechaContacto)
                throw new ArgumentException("El próximo contacto debe ser posterior a la fecha de contacto");
        }
    }
}