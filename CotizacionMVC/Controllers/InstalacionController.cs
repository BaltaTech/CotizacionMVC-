using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace CotizacionMVC.Controllers
{
    public class InstalacionController : Controller
    {
        private readonly IInstalacionServicio _instalacionServicio;

        public InstalacionController(IInstalacionServicio instalacionServicio)
        {
            _instalacionServicio = instalacionServicio;
        }

        public async Task<IActionResult> Catalogo()
        {
            var viewModel = await _instalacionServicio.ObtenerCatalogoAsync();
            return View(viewModel);
        }
    }
}