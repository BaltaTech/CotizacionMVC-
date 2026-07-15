using CotizacionMVC.Models.Reglas;
using CotizacionMVC.Models.Valor;

namespace CotizacionMVC.Models.Entidades
{
    public class ItemCotizacion
    {
        public Guid Id { get; private set; }
        public Guid CotizacionId { get; private set; }
        public virtual Cotizacion Cotizacion { get; private set; }
        public Guid EquipoId { get; private set; }
        public virtual Equipo Equipo { get; private set; }
        public int Cantidad { get; private set; }
        public Dinero PrecioUnitario { get; private set; }
        public Dinero Subtotal { get; private set; }
        public decimal UtilidadEmpresaPorcentaje { get; private set; }
        public decimal UtilidadVendedorPorcentaje { get; private set; }
        public string? DescripcionPersonalizada { get; private set; }

        protected ItemCotizacion()
        {
            Cotizacion = null!;
            Equipo = null!;
            PrecioUnitario = null!;
            Subtotal = null!;
            DescripcionPersonalizada = null;
        }

        public ItemCotizacion(
            Cotizacion cotizacion,
            Equipo equipo,
            int cantidad,
            decimal utilidadEmpresaPorcentaje,
            decimal utilidadVendedorPorcentaje,
            string? descripcionPersonalizada = null)
        {
            if (cotizacion == null)
                throw new ArgumentNullException(nameof(cotizacion));

            if (equipo == null)
                throw new ArgumentNullException(nameof(equipo));

            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero");

            if (utilidadEmpresaPorcentaje < 0)
                throw new ArgumentException("La utilidad de la empresa no puede ser negativa");

            if (utilidadVendedorPorcentaje < 0)
                throw new ArgumentException("La utilidad del vendedor no puede ser negativa");

            Id = Guid.NewGuid();
            Cotizacion = cotizacion;
            CotizacionId = cotizacion.Id;
            Equipo = equipo;
            EquipoId = equipo.Id;
            Cantidad = cantidad;
            UtilidadEmpresaPorcentaje = utilidadEmpresaPorcentaje;
            UtilidadVendedorPorcentaje = utilidadVendedorPorcentaje;
            DescripcionPersonalizada = descripcionPersonalizada?.Trim();

            CalcularPrecio();
            Subtotal = PrecioUnitario.Multiplicar(Cantidad);
        }

        public void ActualizarCantidad(int nuevaCantidad)
        {
            if (nuevaCantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero");
            Cantidad = nuevaCantidad;
            Subtotal = PrecioUnitario.Multiplicar(Cantidad);
        }

        public void ActualizarDescripcion(string? descripcion)
        {
            if (!string.IsNullOrWhiteSpace(descripcion))
                DescripcionPersonalizada = descripcion.Trim();
        }

        public void RecalcularPrecio()
        {
            CalcularPrecio();
            Subtotal = PrecioUnitario.Multiplicar(Cantidad);
        }

        public string ObtenerDescripcionMostrable()
        {
            if (!string.IsNullOrWhiteSpace(DescripcionPersonalizada))
                return DescripcionPersonalizada;
            return $"{Equipo.Marca} {Equipo.Modelo} - {Equipo.CapacidadToneladas} TR";
        }

        private void CalcularPrecio()
        {
            var precioUnitarioMxn = ReglasNegocio.CalcularPrecioUnitarioMxn(
                Equipo.Marca.ToString(),
                Equipo.PrecioBase,
                Equipo.MonedaOriginal,
                Cotizacion.ObtenerTipoCambioActual(),
                UtilidadEmpresaPorcentaje,
                UtilidadVendedorPorcentaje
            );

            PrecioUnitario = new Dinero(precioUnitarioMxn, Cotizacion.Empresa.MonedaBase);
        }
    }
}