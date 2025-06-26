using Business.Services;
using Entities;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace VineyardApp.BackgroundServices
{
    public class DesiredActualReconciler : BackgroundService
    {
        private readonly IServiceProvider _sp;
        private readonly IMessagePublisher _pub;
        private readonly ILogger<DesiredActualReconciler> _logger;

        public DesiredActualReconciler(
            IServiceProvider sp,
            IMessagePublisher pub,
            ILogger<DesiredActualReconciler> logger)
        {
            _sp = sp;
            _pub = pub;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken ct)
        {
            while (!ct.IsCancellationRequested)
            {
                try
                {
                    using var scope = _sp.CreateScope();
                    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
                    var now = DateTime.UtcNow;

                    // Find pumps still out of sync more than 30s after desired change
                    var cutoff = DateTime.UtcNow.AddSeconds(-30);
                    var toRetry = await db.Pumps
                        .Include(p => p.IoTDevice)
                        .Where(p =>
                            p.IsManualOverride == false  // skip manual overrides
                            && p.DesiredState != p.ActualState               // any mismatch
                            && p.LastDesiredChange < cutoff
                            && p.RetryCount < 3
                        )
                        .ToListAsync(ct);

                    foreach (var pump in toRetry)
                    {
                        var deviceId = pump.IoTDevice.DeviceIdentifier;
                        var topic = $"vineyard/{deviceId}/command";
                        var payload = JsonSerializer.Serialize(new { desired = pump.DesiredState });

                        _logger.LogInformation("Re-publishing desired={Desired} for pump {PumpId}", pump.DesiredState, pump.Id);
                        await _pub.PublishAsync(topic, payload);

                        pump.RetryCount++;
                        pump.LastRetry = DateTime.UtcNow;
                        if (pump.RetryCount >= 3)
                        {
                            pump.NeedsAttention = true;
                            pump.DesiredState = pump.ActualState;    // clear the mismatch
                            pump.LastDesiredChange = null;             // so the user can toggle immediately
                            _logger.LogWarning("Pump {PumpId} still out of sync after 3 retries", pump.Id);
                        }
                    }

                    if (toRetry.Any())
                        await db.SaveChangesAsync(ct);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error in DesiredActualReconciler");
                }

                // run every 30 seconds (adjust to match your heartbeat cadence)
                await Task.Delay(TimeSpan.FromSeconds(60), ct);
            }
        }
    }
}
