using CotizacionMVC.Models.Enums;

namespace CotizacionMVC.Models.Entidades
{
    public class Equipo
    {
        public Guid Id { get; private set; }
        public TipoMarca Marca { get; private set; }
        public string Modelo { get; private set; }
        public string? Tipo { get; private set; }
        public decimal CapacidadToneladas { get; private set; }
        public string? Tension { get; private set; }
        public string? Tecnologia { get; private set; }
        public decimal PrecioBase { get; private set; }
        public string MonedaOriginal { get; private set; }
        public bool Activo { get; private set; }
        public DateTime FechaCreacion { get; private set; }

        // Constructor público sin parámetros para Model Binding de ASP.NET Core
        public Equipo()
        {
            Modelo = null!;
            MonedaOriginal = null!;
            Tipo = null;
            Tension = null;
            Tecnologia = null;
        }

    

        public Equipo(
            TipoMarca marca,
            string modelo,
            decimal capacidadToneladas,
            decimal precioBase,
            string monedaOriginal)
        {
            if (string.IsNullOrWhiteSpace(modelo))
                throw new ArgumentException("El modelo es obligatorio");

            if (capacidadToneladas <= 0)
                throw new ArgumentException("La capacidad debe ser mayor a cero");

            if (precioBase <= 0)
                throw new ArgumentException("El precio base debe ser mayor a cero");

            if (monedaOriginal != "MXN" && monedaOriginal != "USD")
                throw new ArgumentException("La moneda debe ser MXN o USD");

            // Regla: Trane y York solo en USD
            if ((marca == TipoMarca.Trane || marca == TipoMarca.York) && monedaOriginal != "USD")
                throw new InvalidOperationException("Los equipos Trane y York solo pueden tener precio en USD");

            // Regla: Las demás marcas solo en MXN
            if (marca != TipoMarca.Trane && marca != TipoMarca.York && monedaOriginal != "MXN")
                throw new InvalidOperationException($"Los equipos {marca} solo pueden tener precio en MXN");

            Id = Guid.NewGuid();
            Marca = marca;
            Modelo = modelo.Trim();
            CapacidadToneladas = capacidadToneladas;
            PrecioBase = precioBase;
            MonedaOriginal = monedaOriginal;
            Activo = true;
            FechaCreacion = DateTime.UtcNow;

            Tipo = null;
            Tension = null;
            Tecnologia = null;
        }

        public void CompletarDetalles(string tipo, string tension, string tecnologia)
        {
            Tipo = tipo ?? throw new ArgumentNullException(nameof(tipo));
            Tension = tension ?? throw new ArgumentNullException(nameof(tension));
            Tecnologia = tecnologia ?? throw new ArgumentNullException(nameof(tecnologia));
        }

        public void Desactivar()
        {
            Activo = false;
        }

        public void Activar()
        {
            Activo = true;
        }

        public void ActualizarPrecio(decimal nuevoPrecio)
        {
            if (nuevoPrecio <= 0)
                throw new ArgumentException("El precio debe ser mayor a cero");

            PrecioBase = nuevoPrecio;
        }

        public bool EsMarcaTrane()
        {
            return Marca == TipoMarca.Trane;
        }

        public bool TieneDetallesCompletos()
        {
            return !string.IsNullOrWhiteSpace(Tipo) &&
                   !string.IsNullOrWhiteSpace(Tension) &&
                   !string.IsNullOrWhiteSpace(Tecnologia);
        }
    }
}