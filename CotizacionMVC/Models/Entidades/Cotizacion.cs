using CotizacionMVC.Models.Enums;
using CotizacionMVC.Models.Valor;
using System.Reflection.Metadata;

namespace CotizacionMVC.Models.Entidades
{
    public class Cotizacion
    {
        private readonly List<ItemCotizacion> _itemsEquipos = new List<ItemCotizacion>();
        private readonly List<ItemInstalacion> _itemsInstalacion = new List<ItemInstalacion>();
        private readonly List<Seguimiento> _seguimientos = new List<Seguimiento>();

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

        public string? RutaPdf { get; private set; }

        public IReadOnlyCollection<ItemCotizacion> ItemsEquipos => _itemsEquipos.AsReadOnly();
        public IReadOnlyCollection<ItemInstalacion> ItemsInstalacion => _itemsInstalacion.AsReadOnly();
        public IReadOnlyCollection<Seguimiento> Seguimientos => _seguimientos.AsReadOnly();

        // Constructor protegido para EF Core - TODAS las propiedades obligatorias se inicializan
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
        }

        public Cotizacion(
            string numeroCotizacion,
            Cliente cliente,
            Empresa empresa,
            Usuario vendedor,
            decimal areaMetrosCuadrados,
            string condicionesPago)
        {
            if (string.IsNullOrWhiteSpace(numeroCotizacion))
                throw new ArgumentException("El número de cotización es obligatorio");

            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));

            if (empresa == null)
                throw new ArgumentNullException(nameof(empresa));

            if (vendedor == null)
                throw new ArgumentNullException(nameof(vendedor));

            if (areaMetrosCuadrados <= 0)
                throw new ArgumentException("El área debe ser mayor a cero");

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

            // Inicializar en cero
            Subtotal = new Dinero(0, empresa.MonedaBase);
            Iva = new Dinero(0, empresa.MonedaBase);
            Total = new Dinero(0, empresa.MonedaBase);
            RequiereAutorizacion = false;
        }

        public void AgregarEquipo(Equipo equipo, int cantidad, decimal utilidadEmpresa, decimal utilidadVendedor, string? descripcionPersonalizada = null)
        {
            if (equipo == null)
                throw new ArgumentNullException(nameof(equipo));

            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero");

            // Regla: Si la empresa es exclusiva Trane, solo se pueden agregar equipos Trane
            if (Empresa.EsExclusivaTrane && !equipo.EsMarcaTrane())
                throw new InvalidOperationException($"Esta empresa solo puede cotizar equipos Trane. El equipo {equipo.Marca} no está permitido.");

            if (!equipo.Activo)
                throw new InvalidOperationException($"El equipo {equipo.Modelo} no está activo en el catálogo");

            var item = new ItemCotizacion(this, equipo, cantidad, utilidadEmpresa, utilidadVendedor, descripcionPersonalizada);
            _itemsEquipos.Add(item);
            RecalcularTotales();
        }

        public void AgregarInstalacion(string concepto, string? descripcion, int cantidad, decimal costoUnitario)
        {
            if (string.IsNullOrWhiteSpace(concepto))
                throw new ArgumentException("El concepto es obligatorio");

            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero");

            if (costoUnitario < 0)
                throw new ArgumentException("El costo unitario no puede ser negativo");

            var item = new ItemInstalacion(this, concepto, descripcion ?? string.Empty, cantidad, costoUnitario);
            _itemsInstalacion.Add(item);
            RecalcularTotales();
        }

        public void AgregarInstalacionPredefinida(Instalacion instalacion, int cantidad)
        {
            if (instalacion == null)
                throw new ArgumentNullException(nameof(instalacion));

            if (!instalacion.Activo)
                throw new InvalidOperationException($"La instalación {instalacion.Concepto} no está activa");

            AgregarInstalacion(instalacion.Concepto, instalacion.Descripcion, cantidad, instalacion.CostoUnitario);
        }

        public void CambiarEstado(EstadoCotizacion nuevoEstado)
        {
            ValidarTransicionEstado(nuevoEstado);
            Estado = nuevoEstado;

            // Si se aceptó o pagó anticipo, ya no se puede modificar
            if (nuevoEstado == EstadoCotizacion.Aceptada ||
                nuevoEstado == EstadoCotizacion.PagoAnticipo ||
                nuevoEstado == EstadoCotizacion.Cerrada ||
                nuevoEstado == EstadoCotizacion.Perdida)
            {
                // La cotización queda congelada
            }
        }

        private void ValidarTransicionEstado(EstadoCotizacion nuevoEstado)
        {
            var estadoActual = Estado;
            var estadoActualNum = (int)estadoActual;
            var nuevoEstadoNum = (int)nuevoEstado;

            // No se puede retroceder a un estado anterior
            if (nuevoEstadoNum < estadoActualNum && estadoActualNum > 0 && nuevoEstadoNum > 0)
                throw new InvalidOperationException($"No se puede cambiar del estado {estadoActual} a {nuevoEstado} (solo se puede avanzar)");

            // No se puede modificar una cotización cerrada o cancelada
            if (estadoActual == EstadoCotizacion.Cerrada || estadoActual == EstadoCotizacion.Perdida)
                throw new InvalidOperationException($"No se puede modificar una cotización {estadoActual}");

            // No se puede modificar una cotización aceptada
            if (estadoActual == EstadoCotizacion.Aceptada && nuevoEstado != EstadoCotizacion.PagoAnticipo)
                throw new InvalidOperationException("Una cotización aceptada solo puede pasar a pago anticipo");
        }

        private void RecalcularTotales()
        {
            var subtotalEquipos = _itemsEquipos.Sum(i => i.Subtotal.Monto);
            var subtotalInstalaciones = _itemsInstalacion.Sum(i => i.Subtotal.Monto);
            var subtotalMonto = subtotalEquipos + subtotalInstalaciones;

            Subtotal = new Dinero(subtotalMonto, Empresa.MonedaBase);

            var ivaMonto = subtotalMonto * 0.16m;
            Iva = new Dinero(ivaMonto, Empresa.MonedaBase);

            var totalMonto = subtotalMonto + ivaMonto;
            Total = new Dinero(totalMonto, Empresa.MonedaBase);

            // Regla: Cotizaciones mayores a $500,000 MXN requieren autorización
            var totalEnMxn = Empresa.MonedaBase == "MXN"
                ? Total
                : Total.ConvertirA("MXN", ObtenerTipoCambioActual());

            RequiereAutorizacion = totalEnMxn.Monto > 500000;
        }

        public decimal ObtenerTipoCambioActual()
        {
            return 20.50m;
        }

        public bool PuedeSerModificada()
        {
            return Estado != EstadoCotizacion.Aceptada &&
                   Estado != EstadoCotizacion.PagoAnticipo &&
                   Estado != EstadoCotizacion.Cerrada &&
                   Estado != EstadoCotizacion.Perdida;
        }

        public int ObtenerPorcentajeEstado()
        {
            return (int)Estado;
        }

        public void ActualizarDatosBasicos(Cliente cliente, decimal areaMetrosCuadrados, string condicionesPago)
        {
            if (!PuedeSerModificada())
                throw new InvalidOperationException("La cotización no puede ser modificada en su estado actual");

            if (cliente == null)
                throw new ArgumentNullException(nameof(cliente));

            if (areaMetrosCuadrados <= 0)
                throw new ArgumentException("El área debe ser mayor a cero");

            Cliente = cliente;
            ClienteId = cliente.Id;
            AreaMetrosCuadrados = areaMetrosCuadrados;
            CondicionesPago = condicionesPago ?? string.Empty;
        }

        public void GuardarRutaPdf(string ruta)
        {
            
            RutaPdf = ruta;
        }            
    }
}