namespace CotizacionMVC.Models.Entidades
{
    public class Instalacion
    {
        public Guid Id { get; private set; }
        public string Concepto { get; private set; }
        public string? Descripcion { get; private set; }  
        public decimal CostoUnitario { get; private set; }
        public string Categoria { get; private set; }
        public bool Activo { get; private set; }

        // Constructor protegido para EF Core
        protected Instalacion()
        {
            Concepto = null!;
            Categoria = null!;
            Descripcion = null;
        }

        public Instalacion(string concepto, string? descripcion, decimal costoUnitario, string categoria)
        {
            if (string.IsNullOrWhiteSpace(concepto))
                throw new ArgumentException("El concepto es obligatorio");

            if (string.IsNullOrWhiteSpace(categoria))
                throw new ArgumentException("La categoría es obligatoria");

            if (costoUnitario < 0)
                throw new ArgumentException("El costo unitario no puede ser negativo");

            Id = Guid.NewGuid();
            Concepto = concepto.Trim();
            Descripcion = descripcion?.Trim();
            CostoUnitario = costoUnitario;
            Categoria = categoria.Trim();
            Activo = true;
        }

        public void Desactivar()
        {
            Activo = false;
        }

        public void Activar()
        {
            Activo = true;
        }

        public void ActualizarCosto(decimal nuevoCosto)
        {
            if (nuevoCosto < 0)
                throw new ArgumentException("El costo no puede ser negativo");

            CostoUnitario = nuevoCosto;
        }

        public void ActualizarDescripcion(string? descripcion)
        {
            if (!string.IsNullOrWhiteSpace(descripcion))
                Descripcion = descripcion.Trim();
            else
                Descripcion = null;
        }

        public void ActualizarCategoria(string categoria)
        {
            if (string.IsNullOrWhiteSpace(categoria))
                throw new ArgumentException("La categoría es obligatoria");

            Categoria = categoria.Trim();
        }

        public string ObtenerDescripcionMostrable()
        {
            if (!string.IsNullOrWhiteSpace(Descripcion))
                return Descripcion;

            return Concepto;
        }

        public bool TieneDescripcion()
        {
            return !string.IsNullOrWhiteSpace(Descripcion);
        }
    }
}