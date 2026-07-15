using CotizacionMVC.Data;
using CotizacionMVC.Models.Entidades;
using Microsoft.EntityFrameworkCore;

namespace CotizacionMVC.Servicios.Infraestructura
{
    public class RecordatorioBackgroundService : BackgroundService
    {
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<RecordatorioBackgroundService> _logger;

        public RecordatorioBackgroundService(
            IServiceScopeFactory scopeFactory,
            ILogger<RecordatorioBackgroundService> logger)
        {
            _scopeFactory = scopeFactory;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("RecordatorioBackgroundService iniciado.");

            while (!stoppingToken.IsCancellationRequested)
            {
                try
                {
                    await RevisarRecordatoriosAsync();
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error al revisar recordatorios.");
                }

                // Revisar cada 15 minutos
                await Task.Delay(TimeSpan.FromMinutes(15), stoppingToken);
            }
        }

        private async Task RevisarRecordatoriosAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var context = scope.ServiceProvider.GetRequiredService<ApplicationDbContext>();
            var notificacionServicio = scope.ServiceProvider.GetRequiredService<NotificacionServicio>();

            var ahora = DateTime.UtcNow;
            var enUnaHora = ahora.AddHours(1);

            // 1. Recordatorios próximos (dentro de 1 hora)
            var proximos = await context.Seguimientos
                .Include(s => s.Lead)
                .Include(s => s.Cotizacion)
                .Where(s => s.ProximoContacto.HasValue
                    && s.ProximoContacto.Value <= enUnaHora
                    && s.ProximoContacto.Value > ahora
                    && !s.RecordatorioEnviado)
                .ToListAsync();

            foreach (var seg in proximos)
            {
                var referencia = seg.Lead?.NombreContacto
                    ?? seg.Cotizacion?.NumeroCotizacion
                    ?? "Seguimiento";

                await notificacionServicio.EnviarNotificacionAsync(
                    seg.VendedorId.ToString(),
                    " Recordatorio de Seguimiento",
                    $"Tienes un seguimiento en 1 hora: {referencia} - {seg.ProximoContacto:HH:mm}",
                    "warning");

                seg.MarcarRecordatorioEnviado();
                _logger.LogInformation("Recordatorio enviado: {Id}", seg.Id);
            }

            // 2. Seguimientos vencidos
            var vencidos = await context.Seguimientos
                .Include(s => s.Lead)
                .Include(s => s.Cotizacion)
                .Where(s => s.ProximoContacto.HasValue
                    && s.ProximoContacto.Value < ahora
                    && !s.RecordatorioEnviado)
                .ToListAsync();

            foreach (var seg in vencidos)
            {
                var referencia = seg.Lead?.NombreContacto
                    ?? seg.Cotizacion?.NumeroCotizacion
                    ?? "Seguimiento";

                await notificacionServicio.EnviarNotificacionAsync(
                    seg.VendedorId.ToString(),
                    "Seguimiento Vencido",
                    $"Se te pasó un seguimiento agendado: {referencia} - {seg.ProximoContacto:dd/MM HH:mm}",
                    "danger");

                seg.MarcarRecordatorioEnviado();
                _logger.LogInformation("Alerta vencido enviada: {Id}", seg.Id);
            }

            if (proximos.Any() || vencidos.Any())
                await context.SaveChangesAsync();
        }
    }
}