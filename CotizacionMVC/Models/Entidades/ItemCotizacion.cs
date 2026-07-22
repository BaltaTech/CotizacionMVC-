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
        public Dinero PrecioUnitarioUSD { get; private set; }
        public Dinero SubtotalUSD { get; private set; }
        public decimal FactorPrecio { get; private set; }
        public decimal FactorUtilidad { get; private set; }
        public string? DescripcionPersonalizada { get; private set; }

        // Constructor protegido para EF Core
        protected ItemCotizacion()
        {
            Cotizacion = null!;
            Equipo = null!;
            PrecioUnitario = null!;
            Subtotal = null!;
            PrecioUnitarioUSD = null!;
            SubtotalUSD = null!;
            DescripcionPersonalizada = null;
        }

        public ItemCotizacion(
            Cotizacion cotizacion,
            Equipo equipo,
            int cantidad,
            decimal factorPrecio,
            decimal factorUtilidad,
            string? descripcionPersonalizada = null)
        {
            if (cotizacion == null)
                throw new ArgumentNullException(nameof(cotizacion));

            if (equipo == null)
                throw new ArgumentNullException(nameof(equipo));

            if (cantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero");

            if (factorPrecio <= 0)
                throw new ArgumentException("El factor de precio debe ser mayor a cero");

            if (factorUtilidad <= 0)
                throw new ArgumentException("El factor de utilidad debe ser mayor a cero");

            Id = Guid.NewGuid();
            Cotizacion = cotizacion;
            CotizacionId = cotizacion.Id;
            Equipo = equipo;
            EquipoId = equipo.Id;
            Cantidad = cantidad;
            FactorPrecio = factorPrecio;
            FactorUtilidad = factorUtilidad;
            DescripcionPersonalizada = descripcionPersonalizada?.Trim();

            CalcularPrecio();
        }

        public void ActualizarCantidad(int nuevaCantidad)
        {
            if (nuevaCantidad <= 0)
                throw new ArgumentException("La cantidad debe ser mayor a cero");
            Cantidad = nuevaCantidad;
            ActualizarSubtotales();
        }

        public void ActualizarDescripcion(string? descripcion)
        {
            if (!string.IsNullOrWhiteSpace(descripcion))
                DescripcionPersonalizada = descripcion.Trim();
        }

        public void RecalcularPrecio()
        {
            CalcularPrecio();
        }

        public string ObtenerDescripcionMostrable()
        {
            if (!string.IsNullOrWhiteSpace(DescripcionPersonalizada))
                return DescripcionPersonalizada;
            return $"{Equipo.Marca} {Equipo.Modelo} - {Equipo.CapacidadToneladas} TR";
        }

        private void CalcularPrecio()
        {
            // Obtener la calculadora según la marca del equipo
            var calculadora = ReglasNegocio.ObtenerCalculadora(
                Equipo.Marca.ToString()
            );

            // Calcular precios usando la estrategia adecuada
            var (precioUSD, precioMXN) = calculadora.Calcular(
                Equipo.PrecioBase,
                Equipo.MonedaOriginal,
                Cotizacion.ObtenerTipoCambioActual(),
                FactorPrecio,
                FactorUtilidad
            );

            PrecioUnitarioUSD = new Dinero(precioUSD, "USD");
            PrecioUnitario = new Dinero(precioMXN, Cotizacion.Empresa.MonedaBase);

            ActualizarSubtotales();
        }

        private void ActualizarSubtotales()
        {
            SubtotalUSD = PrecioUnitarioUSD.Multiplicar(Cantidad);
            Subtotal = PrecioUnitario.Multiplicar(Cantidad);
        }
    }
}