using CotizacionMVC.Data;
using CotizacionMVC.Hubs;
using CotizacionMVC.Models.Entidades;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Servicios.Infraestructura
{
    public class NotificacionServicio
    {
        private readonly IHubContext<NotificacionHub> _hubContext;
        private readonly ApplicationDbContext _context;

        public NotificacionServicio(IHubContext<NotificacionHub> hubContext, ApplicationDbContext context)
        {
            _hubContext = hubContext;
            _context = context;
        }

        public async Task EnviarNotificacionAsync(string userId, string titulo, string mensaje, string tipo = "info", string? url = null)
        {
            // 1. Guardar en BD
            var notificacion = new Notificacion(Guid.Parse(userId), titulo, mensaje, tipo, url);
            _context.Notificaciones.Add(notificacion);
            await _context.SaveChangesAsync();

            // 2. Enviar en tiempo real
            await _hubContext.Clients.Group(userId).SendAsync("RecibirNotificacion", new
            {
                Id = notificacion.Id,
                Titulo = titulo,
                Mensaje = mensaje,
                Tipo = tipo,
                Url = url,
                Leida = false,
                Fecha = notificacion.FechaCreacion.ToString("HH:mm")
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

        public async Task<List<Notificacion>> ObtenerNoLeidasAsync(Guid usuarioId)
        {
            return await _context.Notificaciones
                .Where(n => n.UsuarioId == usuarioId && !n.Leida)
                .OrderByDescending(n => n.FechaCreacion)
                .Take(20)
                .ToListAsync();
        }

        public async Task<int> ContarNoLeidasAsync(Guid usuarioId)
        {
            return await _context.Notificaciones
                .CountAsync(n => n.UsuarioId == usuarioId && !n.Leida);
        }

        public async Task MarcarLeidaAsync(Guid notificacionId)
        {
            var notif = await _context.Notificaciones.FindAsync(notificacionId);
            if (notif != null && !notif.Leida)
            {
                notif.MarcarLeida();
                await _context.SaveChangesAsync();
            }
        }

        public async Task MarcarTodasLeidasAsync(Guid usuarioId)
        {
            var noLeidas = await _context.Notificaciones
                .Where(n => n.UsuarioId == usuarioId && !n.Leida)
                .ToListAsync();

            foreach (var n in noLeidas) n.MarcarLeida();
            await _context.SaveChangesAsync();
        }
    }
}