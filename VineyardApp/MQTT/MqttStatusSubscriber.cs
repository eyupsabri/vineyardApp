using Business.Services;
using Entities.DTOs;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using System.Text;
using System.Text.Json;

namespace VineyardApp.MQTT
{
    public class MqttStatusSubscriber : BackgroundService
    {
        private readonly IMqttClient _mqtt;
        private readonly MqttOptions _opts;
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly ILogger<MqttStatusSubscriber> _logger;

        public MqttStatusSubscriber(
            IMqttClient mqtt,
            IOptions<MqttOptions> opts,
            IServiceScopeFactory scopeFactory,
            ILogger<MqttStatusSubscriber> logger)
        {
            _mqtt = mqtt;
            _opts = opts.Value;
            _scopeFactory = scopeFactory;
            _logger = logger;
        }
        public override async Task StartAsync(CancellationToken ct)
        {
            try
            {
                _logger.LogInformation("[MQTT-DIAG] StartAsync() fired");
                _logger.LogInformation(
                    "[MQTT-OPTS] Host={Host}, Port={Port}, User={User}, Pass={Pass}",
                    _opts.Host,
                    _opts.Port,
                    _opts.Username ?? "<null>",
                    _opts.Password != null ? new string('*', _opts.Password.Length) : "<null>"
                );

                // Now call the base so ExecuteAsync runs:
                await base.StartAsync(ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MQTT-DIAG] StartAsync failed");
            }

        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {

            // Build client options
            var builder = new MqttClientOptionsBuilder()
                .WithTcpServer(_opts.Host, _opts.Port)
                .WithCredentials(_opts.Username, _opts.Password);

            if (_opts.UseTls)
            {
                _logger.LogInformation("[MQTT] Enabling TLS");
                builder = builder.WithTls();
            }

            var clientOptions = builder
                .WithCleanSession()
                .Build();

            // Connected event
            _mqtt.ConnectedAsync += async e =>
            {
                _logger.LogInformation("[MQTT] Connected to {Host}:{Port}", _opts.Host, _opts.Port);
                await _mqtt.SubscribeAsync(new MqttTopicFilterBuilder()
                    .WithTopic("vineyard/+/updatestatus")
                    .WithQualityOfServiceLevel(MqttQualityOfServiceLevel.AtLeastOnce)
                    .Build());
                _logger.LogInformation("[MQTT] Subscribed to 'vineyard/+/updatestatus'");
            };

            // Disconnected event
            _mqtt.DisconnectedAsync += e =>
            {
                _logger.LogWarning("[MQTT] Disconnected: {Reason}", e.Reason);
                return Task.CompletedTask;
            };

            // Message received
            _mqtt.ApplicationMessageReceivedAsync += async e =>
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);
                _logger.LogInformation("[MQTT] Received on '{Topic}': {Payload}", topic, payload);


                StatusPayload statusDto;
                try
                {
                    statusDto = JsonSerializer.Deserialize<StatusPayload>(payload, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })! ?? throw new JsonException("Payload deserialized to null");
                }
                catch (Exception je)
                {
                    _logger.LogWarning(je, "[MQTT] Incomplete payload—aborting: {Payload}", payload);
                    return;
                }

                //if (dto is null)
                //{
                //    _logger.LogWarning("[MQTT] Failed to deserialize payload: {Payload}", payload);
                //    return;
                //}

                var parts = topic.Split('/');
                if (parts.Length != 3 || !Guid.TryParse(parts[1], out var deviceGuid))
                    return;

                var dto = new UpdateStatusRequestDTO
                {
                    DeviceIdentifier = deviceGuid,
                    ActualState = statusDto.ActualState,
                    TriggeredBy = statusDto.TriggeredBy
                };

                using var scope = _scopeFactory.CreateScope();
                var service = scope.ServiceProvider.GetRequiredService<IIoTDevicesService>();

                try
                {
                    var result = await service.UpdateDeviceStatus(dto);
                    _logger.LogInformation("[MQTT] Updated status for device {DeviceId} with status = {status}", dto.DeviceIdentifier, result);
                }
                catch (DbUpdateException dbEx)
                {
                    _logger.LogWarning(dbEx, "[MQTT] Failed to update device {DeviceId} (DB error)—ignoring", dto.DeviceIdentifier);
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "[MQTT] Unexpected error updating device {DeviceId}", dto.DeviceIdentifier);
                }
            };

            // Connect
            try
            {
                await _mqtt.ConnectAsync(clientOptions, stoppingToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "[MQTT] Connection failed");
                //throw;
            }

            // Keep running until stopped
            await Task.Delay(Timeout.Infinite, stoppingToken);
        }
    }
}
