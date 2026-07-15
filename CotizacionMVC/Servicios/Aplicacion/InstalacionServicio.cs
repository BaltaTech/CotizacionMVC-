using CotizacionMVC.Data.Repositorios.Interfaces;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using CotizacionMVC.ViewModels.Instalacion;

namespace CotizacionMVC.Servicios.Aplicacion
{
    public class InstalacionServicio : IInstalacionServicio
    {
        private readonly IInstalacionRepository _instalacionRepository;

        public InstalacionServicio(IInstalacionRepository instalacionRepository)
        {
            _instalacionRepository = instalacionRepository;
        }

        public async Task<InstalacionCatalogoViewModel> ObtenerCatalogoAsync()
        {
            var instalaciones = await _instalacionRepository.GetAllAsync();

            var viewModel = new InstalacionCatalogoViewModel
            {
                Categorias = instalaciones
                    .Where(i => i.Activo)
                    .GroupBy(i => i.Categoria)
                    .OrderBy(g => g.Key)
                    .Select(g => new InstalacionCategoriaViewModel
                    {
                        Categoria = g.Key,
                        Items = g.Select(i => new InstalacionItemViewModel
                        {
                            Id = i.Id,
                            Concepto = i.Concepto,
                            Descripcion = i.Descripcion,
                            CostoUnitario = i.CostoUnitario
                        }).OrderBy(i => i.Concepto).ToList()
                    }).ToList()
            };

            return viewModel;
        }
    }
}