using CotizacionMVC.Models.Entidades;
namespace CotizacionMVC.Servicios
{
    public interface IDocumento
    {
        byte[] Generar(Cotizacion cotizacion);
        string TipoContenido { get; }
        string ExtensionArchivo { get;  }

    }
}
