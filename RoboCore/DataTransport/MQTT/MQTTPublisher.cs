using System;
using System.Threading;
using System.Threading.Tasks;
using MQTTnet;
using MQTTnet.Client;
using MQTTnet.Protocol;
using Serilog;

using RoboCore.Messages;

namespace RoboCore.DataTransport.MQTT
{
    public class MQTTPublisher<TMessage> : IPublisher<TMessage>
    {
        private readonly CancellationTokenSource _cancellationToken;
        private readonly MqttQualityOfServiceLevel _qosLevel;
        private readonly IMqttClient _client;
        private readonly PubSubMessageSerializer<PubSubMessage, TMessage> _serializer;
        private readonly string _topic;
        
        public MQTTPublisher(MqttQualityOfServiceLevel qosLevel,
                             IMqttClient client, 
                             PubSubMessageSerializer<PubSubMessage, TMessage> serializer, 
                             string topic)
        {
            _cancellationToken = new CancellationTokenSource();
            _qosLevel = qosLevel;
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _serializer = serializer ?? throw new ArgumentException(nameof(serializer));
            _topic = topic ?? throw new ArgumentNullException(nameof(topic));
        }
        
        public async Task PublishMessage(TMessage message, bool retained = false)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            
            var payload = _serializer.Serialize(message);
            var packedMessage = new MqttApplicationMessageBuilder()
                .WithTopic(_topic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(_qosLevel)
                .WithRetainFlag(retained)
                .Build();

            try
            {
                await _client.PublishAsync(packedMessage, _cancellationToken.Token);
            }
            catch (Exception e)
            {
                Log.Error($"Failed to publish message on topic \"{_topic}\". Exception: {e}");
                throw;
            }
        }

        public void Dispose()
        {
            _cancellationToken.Cancel();
        }
    }
}