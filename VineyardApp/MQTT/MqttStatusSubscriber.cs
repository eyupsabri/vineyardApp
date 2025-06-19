using Business.Services;
using Entities.DTOs;
using Microsoft.Extensions.Options;
using MQTTnet.Client;
using MQTTnet.Protocol;         // ← QoS enum lives here
using System.Text;
using System.Text.Json;

namespace VineyardApp.MQTT
{
    public class MqttStatusSubscriber : BackgroundService
    {
        private readonly IMqttClient _mqtt;
        private readonly MqttOptions _opts;
        private readonly IIoTDevicesService _service;

        public MqttStatusSubscriber(
            IMqttClient mqtt,
            IOptions<MqttOptions> opts,
            IIoTDevicesService service)
        {
            _mqtt = mqtt;
            _opts = opts.Value;
            _service = service;
        }

        public override async Task StartAsync(CancellationToken ct)
        {
            var builder = new MqttClientOptionsBuilder()
                .WithTcpServer(_opts.Host, _opts.Port)
                .WithCredentials(_opts.Username, _opts.Password);

            if (_opts.UseTls)
                builder = builder.WithTls();

            var clientOptions = builder.Build();

            _mqtt.ApplicationMessageReceivedAsync += async e =>
            {
                var topic = e.ApplicationMessage.Topic;
                var payload = Encoding.UTF8.GetString(e.ApplicationMessage.Payload);

                var dto = JsonSerializer.Deserialize<UpdateStatusRequestDTO>(payload);
                if (dto is null) return;

                var parts = topic.Split('/');
                if (parts.Length == 3 && Guid.TryParse(parts[1], out var deviceGuid))
                    dto.DeviceIdentifier = deviceGuid;

                await _service.UpdateDeviceStatus(dto);
            };

            await _mqtt.ConnectAsync(clientOptions, ct);
            await _mqtt.SubscribeAsync("vineyard/+/status", MqttQualityOfServiceLevel.AtLeastOnce);
        }

        protected override Task ExecuteAsync(CancellationToken stoppingToken)
            => Task.CompletedTask;
    }
}
