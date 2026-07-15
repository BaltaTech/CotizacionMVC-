using CotizacionMVC.Servicios.Infraestructura;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace CotizacionMVC.Controllers
{
    [Authorize]
    public class NotificacionController : Controller
    {
        private readonly NotificacionServicio _notificacionServicio;

        public NotificacionController(NotificacionServicio notificacionServicio)
        {
            _notificacionServicio = notificacionServicio;
        }

        [HttpGet]
        public async Task<IActionResult> MisNotificaciones()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var notificaciones = await _notificacionServicio.ObtenerNoLeidasAsync(Guid.Parse(userId!));
            var resultado = notificaciones.Select(n => new
            {
                n.Id,
                n.Titulo,
                n.Mensaje,
                n.Tipo,
                n.Url,
                n.Leida,
                Fecha = n.FechaCreacion.ToString("HH:mm dd/MM")
            });
            return Json(resultado);
        }

        [HttpPost]
        public async Task<IActionResult> MarcarLeida(Guid id)
        {
            await _notificacionServicio.MarcarLeidaAsync(id);
            return Ok();
        }

        [HttpPost]
        public async Task<IActionResult> MarcarTodasLeidas()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            await _notificacionServicio.MarcarTodasLeidasAsync(Guid.Parse(userId!));
            return Ok();
        }
    }
}