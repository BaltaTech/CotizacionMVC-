using CotizacionMVC.Models.Valor;

namespace CotizacionMVC.Models.Entidades
{
    public class ItemInstalacion
    {
        public Guid Id { get; private set; }
        public Guid CotizacionId { get; private set; }
        public virtual Cotizacion Cotizacion { get; private set; }
        public Guid? InstalacionId { get; private set; }
        public virtual Instalacion? Instalacion { get; private set; }  
        public string Concepto { get; private set; }
        public string? Descripcion { get; private set; }  
        public int Cantidad { get; private set; }
        public Dinero CostoUnitario { get; private set; }
        public Dinero Subtotal { get; private set; }

        // Constructor protegido para EF Core
        protected ItemInstalacion()
        {
            Cotizacion = null!;
            Concepto = null!;
            CostoUnitario = null!;
            Subtotal = null!;
            Descripcion = null;
            Instalacion = null;
        }

        public ItemInstalacion(
            Cotizacion cotizacion,
            string concepto,
            string? descripcion,
            int cantidad,
            decimal costoUnitario,
            Instalacion? instalacion = null)
        {
            if (cotizacion == null)
                throw new ArgumentNullException(nameof(cotizacion));

            if (string.IsNullOrWhiteSpace(concepto))
                throw new ArgumentException("El concepto es obligatorio");

            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero");

            if (costoUnitario < 0)
                throw new ArgumentException("El costo unitario no puede ser negativo");

            Id = Guid.NewGuid();
            Cotizacion = cotizacion;
            CotizacionId = cotizacion.Id;
            Concepto = concepto.Trim();
            Descripcion = descripcion?.Trim();
            Cantidad = cantidad;
            CostoUnitario = new Dinero(costoUnitario, "MXN"); 

            // Convertir a la moneda de la cotización si es necesario
            var costoEnMonedaCotizacion = cotizacion.Empresa.MonedaBase == "MXN"
                ? CostoUnitario
                : CostoUnitario.ConvertirA(cotizacion.Empresa.MonedaBase, cotizacion.ObtenerTipoCambioActual());

            Subtotal = costoEnMonedaCotizacion.Multiplicar(Cantidad);

            if (instalacion != null)
            {
                InstalacionId = instalacion.Id;
                Instalacion = instalacion;
            }
        }

        public void ActualizarCantidad(int nuevaCantidad)
        {
            if (nuevaCantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero");

            Cantidad = nuevaCantidad;

            var costoEnMonedaCotizacion = Cotizacion.Empresa.MonedaBase == "MXN"
                ? CostoUnitario
                : CostoUnitario.ConvertirA(Cotizacion.Empresa.MonedaBase, Cotizacion.ObtenerTipoCambioActual());

            Subtotal = costoEnMonedaCotizacion.Multiplicar(Cantidad);
        }

        public void ActualizarCostoUnitario(decimal nuevoCostoUnitario)
        {
            if (nuevoCostoUnitario < 0)
                throw new ArgumentException("El costo unitario no puede ser negativo");

            CostoUnitario = new Dinero(nuevoCostoUnitario, "MXN");

            var costoEnMonedaCotizacion = Cotizacion.Empresa.MonedaBase == "MXN"
                ? CostoUnitario
                : CostoUnitario.ConvertirA(Cotizacion.Empresa.MonedaBase, Cotizacion.ObtenerTipoCambioActual());

            Subtotal = costoEnMonedaCotizacion.Multiplicar(Cantidad);
        }

        public string ObtenerDescripcionMostrable()
        {
            if (!string.IsNullOrWhiteSpace(Descripcion))
                return Descripcion;

            return Concepto;
        }

        public bool EsPredefinida()
        {
            return InstalacionId.HasValue && Instalacion != null;
        }
    }
}