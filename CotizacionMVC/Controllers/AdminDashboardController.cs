using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using CotizacionMVC.Servicios.Aplicacion.Interfaces;

namespace CotizacionMVC.Controllers
{
    [Authorize(Roles = "Administrador")]
    public class AdminDashboardController : Controller
    {
        private readonly IAdminDashboardServicio _adminDashboardServicio;

        public AdminDashboardController(IAdminDashboardServicio adminDashboardServicio)
        {
            _adminDashboardServicio = adminDashboardServicio;
        }

        [HttpGet]
        public async Task<IActionResult> Index()
        {
            var dashboard = await _adminDashboardServicio.ObtenerDashboardAsync();
            return View(dashboard);
        }
    }
}