using CotizacionMVC.Models.Enums;
using CotizacionMVC.Models.Reglas;
using CotizacionMVC.Models.Valor;

namespace CotizacionMVC.Models.Entidades
{
    public class Cotizacion
    {
        private readonly List<ItemCotizacion> _itemsEquipos = new();
        private readonly List<ItemInstalacion> _itemsInstalacion = new();
        private readonly List<Seguimiento> _seguimientos = new();

        public Guid Id { get; private set; }
        public string NumeroCotizacion { get; private set; }
        public Guid ClienteId { get; private set; }
        public virtual Cliente Cliente { get; private set; }
        public Guid EmpresaId { get; private set; }
        public virtual Empresa Empresa { get; private set; }
        public Guid VendedorId { get; private set; }
        public virtual Usuario Vendedor { get; private set; }
        public DateTime FechaCreacion { get; private set; }
        public DateTime FechaVencimiento { get; private set; }
        public EstadoCotizacion Estado { get; private set; }
        public decimal AreaMetrosCuadrados { get; private set; }
        public string CondicionesPago { get; private set; }
        public Dinero Subtotal { get; private set; }
        public Dinero Iva { get; private set; }
        public Dinero Total { get; private set; }
        public bool RequiereAutorizacion { get; private set; }
        public Guid? LeadId { get; private set; }
        public virtual Lead? Lead { get; private set; }
        public string? RutaPdf { get; private set; }
        public decimal TipoCambio { get; private set; }
        public decimal RecargoCiudadPorcentaje { get; private set; }
        public Dinero RecargoCiudad { get; private set; }

        public IReadOnlyCollection<ItemCotizacion> ItemsEquipos => _itemsEquipos.AsReadOnly();
        public IReadOnlyCollection<ItemInstalacion> ItemsInstalacion => _itemsInstalacion.AsReadOnly();
        public IReadOnlyCollection<Seguimiento> Seguimientos => _seguimientos.AsReadOnly();

        protected Cotizacion()
        {
            NumeroCotizacion = null!;
            Cliente = null!;
            Empresa = null!;
            Vendedor = null!;
            CondicionesPago = null!;
            Subtotal = null!;
            Iva = null!;
            Total = null!;
            RecargoCiudad = null!;
        }

        public Cotizacion(
            string numeroCotizacion,
            Cliente cliente,
            Empresa empresa,
            Usuario vendedor,
            decimal areaMetrosCuadrados,
            string condicionesPago,
            decimal tipoCambio = 17.43m,
            decimal recargoCiudadPorcentaje = 0)
        {
            if (string.IsNullOrWhiteSpace(numeroCotizacion))
                throw new ArgumentException("El número de cotización es obligatorio");
            if (cliente == null) throw new ArgumentNullException(nameof(cliente));
            if (empresa == null) throw new ArgumentNullException(nameof(empresa));
            if (vendedor == null) throw new ArgumentNullException(nameof(vendedor));
            if (areaMetrosCuadrados <= 0)
                throw new ArgumentException("El área debe ser mayor a cero");
            if (tipoCambio <= 0)
                throw new ArgumentException("El tipo de cambio debe ser mayor a cero");
            if (recargoCiudadPorcentaje < 0)
                throw new ArgumentException("El recargo de ciudad no puede ser negativo");

            Id = Guid.NewGuid();
            NumeroCotizacion = numeroCotizacion;
            Cliente = cliente;
            ClienteId = cliente.Id;
            Empresa = empresa;
            EmpresaId = empresa.Id;
            Vendedor = vendedor;
            VendedorId = vendedor.Id;
            FechaCreacion = DateTime.UtcNow;
            FechaVencimiento = DateTime.UtcNow.AddDays(15);
            Estado = EstadoCotizacion.InformacionSolicitada;
            AreaMetrosCuadrados = areaMetrosCuadrados;
            CondicionesPago = condicionesPago ?? string.Empty;
            TipoCambio = tipoCambio;
            RecargoCiudadPorcentaje = recargoCiudadPorcentaje;

            Subtotal = new Dinero(0, empresa.MonedaBase);
            Iva = new Dinero(0, empresa.MonedaBase);
            Total = new Dinero(0, empresa.MonedaBase);
            RecargoCiudad = new Dinero(0, empresa.MonedaBase);
            RequiereAutorizacion = false;
        }

        public void AgregarEquipo(Equipo equipo, int cantidad, decimal utilidadEmpresa, decimal utilidadVendedor, string? descripcionPersonalizada = null)
        {
            if (equipo == null) throw new ArgumentNullException(nameof(equipo));
            if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor a cero");
            if (Empresa.EsExclusivaTrane && !equipo.EsMarcaTrane())
                throw new InvalidOperationException($"Esta empresa solo puede cotizar equipos Trane.");
            if (!equipo.Activo)
                throw new InvalidOperationException($"El equipo {equipo.Modelo} no está activo.");

            var item = new ItemCotizacion(this, equipo, cantidad, utilidadEmpresa, utilidadVendedor, descripcionPersonalizada);
            _itemsEquipos.Add(item);
            RecalcularTotales();
        }

        public void AgregarInstalacion(string concepto, string? descripcion, int cantidad, decimal costoUnitario)
        {
            if (string.IsNullOrWhiteSpace(concepto)) throw new ArgumentException("El concepto es obligatorio");
            if (cantidad <= 0) throw new ArgumentException("La cantidad debe ser mayor a cero");
            if (costoUnitario < 0) throw new ArgumentException("El costo unitario no puede ser negativo");

            var item = new ItemInstalacion(this, concepto, descripcion ?? string.Empty, cantidad, costoUnitario);
            _itemsInstalacion.Add(item);
            RecalcularTotales();
        }

        public void AgregarInstalacionPredefinida(Instalacion instalacion, int cantidad)
        {
            if (instalacion == null) throw new ArgumentNullException(nameof(instalacion));
            if (!instalacion.Activo) throw new InvalidOperationException($"La instalación {instalacion.Concepto} no está activa");
            AgregarInstalacion(instalacion.Concepto, instalacion.Descripcion, cantidad, instalacion.CostoUnitario);
        }

        public void CambiarEstado(EstadoCotizacion nuevoEstado)
        {
            ValidarTransicionEstado(nuevoEstado);
            Estado = nuevoEstado;
        }

        public void ActualizarRecargoCiudad(decimal porcentaje)
        {
            if (porcentaje < 0) throw new ArgumentException("El recargo no puede ser negativo");
            RecargoCiudadPorcentaje = porcentaje;
            RecalcularTotales();
        }

        private void ValidarTransicionEstado(EstadoCotizacion nuevoEstado)
        {
            var estadoActual = Estado;
            var actual = (int)estadoActual;
            var nuevo = (int)nuevoEstado;

            if (nuevo < actual && actual > 0 && nuevo > 0)
                throw new InvalidOperationException($"No se puede cambiar de {estadoActual} a {nuevoEstado}");
            if (estadoActual == EstadoCotizacion.Cerrada || estadoActual == EstadoCotizacion.Perdida)
                throw new InvalidOperationException($"No se puede modificar una cotización {estadoActual}");
            if (estadoActual == EstadoCotizacion.Aceptada && nuevoEstado != EstadoCotizacion.PagoAnticipo)
                throw new InvalidOperationException("Una cotización aceptada solo puede pasar a pago anticipo");
        }

        private void RecalcularTotales()
        {
            var subtotalEquipos = _itemsEquipos.Sum(i => i.Subtotal.Monto);

            var recargo = ReglasNegocio.CalcularRecargoCiudad(subtotalEquipos, RecargoCiudadPorcentaje);
            RecargoCiudad = new Dinero(recargo, Empresa.MonedaBase);

            var subtotalInstalaciones = _itemsInstalacion.Sum(i => i.Subtotal.Monto);
            var subtotalGeneral = subtotalEquipos + recargo + subtotalInstalaciones;

            Subtotal = new Dinero(subtotalGeneral, Empresa.MonedaBase);
            Iva = new Dinero(ReglasNegocio.CalcularIva(subtotalGeneral), Empresa.MonedaBase);
            Total = new Dinero(ReglasNegocio.CalcularTotal(subtotalGeneral), Empresa.MonedaBase);

            var totalEnMxn = Empresa.MonedaBase == "MXN" ? Total : Total.ConvertirA("MXN", TipoCambio);
            RequiereAutorizacion = totalEnMxn.Monto > ReglasNegocio.MONTO_AUTORIZACION_DIRECCION_MXN;
        }

        public decimal ObtenerTipoCambioActual() => TipoCambio;

        public bool PuedeSerModificada()
        {
            return Estado != EstadoCotizacion.Aceptada &&
                   Estado != EstadoCotizacion.PagoAnticipo &&
                   Estado != EstadoCotizacion.Cerrada &&
                   Estado != EstadoCotizacion.Perdida;
        }

        public int ObtenerPorcentajeEstado() => (int)Estado;

        public void ActualizarDatosBasicos(Cliente cliente, decimal areaMetrosCuadrados, string condicionesPago)
        {
            if (!PuedeSerModificada())
                throw new InvalidOperationException("La cotización no puede ser modificada en su estado actual");
            if (cliente == null) throw new ArgumentNullException(nameof(cliente));
            if (areaMetrosCuadrados <= 0) throw new ArgumentException("El área debe ser mayor a cero");

            Cliente = cliente;
            ClienteId = cliente.Id;
            AreaMetrosCuadrados = areaMetrosCuadrados;
            CondicionesPago = condicionesPago ?? string.Empty;
        }

        public void GuardarRutaPdf(string ruta) => RutaPdf = ruta;

        public void VincularLead(Lead lead)
        {
            if (lead == null) throw new ArgumentNullException(nameof(lead));
            Lead = lead;
            LeadId = lead.Id;
        }
    }
}