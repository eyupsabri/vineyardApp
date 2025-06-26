using Business.Services;
using MQTTnet;
using MQTTnet.Client;

namespace VineyardApp.MQTT
{

    public class MqttMessagePublisher : IMessagePublisher
    {
        private readonly IMqttClient _mqtt;

        public MqttMessagePublisher(IMqttClient mqtt)
            => _mqtt = mqtt;

        public Task PublishAsync(string topic, string payload)
        {
            var msg = new MqttApplicationMessageBuilder()
                .WithTopic(topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(MQTTnet.Protocol.MqttQualityOfServiceLevel.AtLeastOnce)
                .Build();

            return _mqtt.PublishAsync(msg);
        }
    }

}
