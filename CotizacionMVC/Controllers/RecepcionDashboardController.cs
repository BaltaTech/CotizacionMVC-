using CotizacionMVC.Servicios.Aplicacion.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace CotizacionMVC.Controllers
{
    [Authorize(Roles = "Administrador,Recepcion")]
    public class RecepcionDashboardController : Controller
    {
        private readonly ISeguimientoServicio _seguimientoServicio;

        public RecepcionDashboardController(ISeguimientoServicio seguimientoServicio)
        {
            _seguimientoServicio = seguimientoServicio;
        }

        public IActionResult Index()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ObtenerDatos()
        {
            var dashboard = await _seguimientoServicio.ObtenerDashboardRecepcionAsync();
            return Json(dashboard);
        }
    }
}