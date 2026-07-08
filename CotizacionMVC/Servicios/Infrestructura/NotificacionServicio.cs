using CotizacionMVC.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace CotizacionMVC.Servicios.Infraestructura
{
  
    public class NotificacionServicio
    {
        private readonly IHubContext<NotificacionHub> _hubContext;

        public NotificacionServicio(IHubContext<NotificacionHub> hubContext)
        {
            _hubContext = hubContext;
        }

        
        public async Task EnviarNotificacionAsync(string userId, string titulo, string mensaje, string tipo = "info")
        {
            await _hubContext.Clients.Group(userId).SendAsync("RecibirNotificacion", new
            {
                Titulo = titulo,
                Mensaje = mensaje,
                Tipo = tipo, // info, success, warning, danger
                Fecha = DateTime.Now.ToString("HH:mm")
            });
        }

        
        public async Task EnviarNotificacionPorRolAsync(string rol, string titulo, string mensaje)
        {
            await _hubContext.Clients.Group(rol).SendAsync("RecibirNotificacion", new
            {
                Titulo = titulo,
                Mensaje = mensaje,
                Tipo = "info",
                Fecha = DateTime.Now.ToString("HH:mm")
            });
        }
    }
}