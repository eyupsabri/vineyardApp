using Entities;
using Microsoft.EntityFrameworkCore;

namespace VineyardApp.BackgroundServices
{
    public class OfflinePumpChecker : BackgroundService
    {
        private readonly IServiceProvider _sp;
        //private readonly INotificationService _notifier;
        private readonly ILogger<OfflinePumpChecker> _logger;

        public OfflinePumpChecker(
            IServiceProvider sp,
            //INotificationService notifier,
            ILogger<OfflinePumpChecker> logger)
        {
            _sp = sp;
            //_notifier = notifier;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                using var scope = _sp.CreateScope();
                var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

                var cutoff = DateTime.UtcNow.AddMinutes(-5);
                var pumps = await db.Pumps
                    .Include(p => p.IoTDevice).ThenInclude(d => d.UserDevices).ThenInclude(ud => ud.User)
                    .Where(p => p.ActualState && p.LastHeartbeat.HasValue && p.LastHeartbeat.Value < cutoff)
                    .ToListAsync(ct);

                foreach (var pump in pumps)
                {
                    _logger.LogWarning("Pump {PumpId} offline since {Cutoff}", pump.Id, cutoff);

                    foreach (var userDevice in pump.IoTDevice.UserDevices)
                    {
                        var user = userDevice.User;
                        //await _notifier.NotifyAsync(
                        //    user.Id,
                        //    $"Your pump “{pump.IoTDevice.Name}” went offline at {cutoff:HH:mm}."
                        //);
                    }
                }

                await Task.Delay(TimeSpan.FromSeconds(300), ct);
            }
        }
    }
}
