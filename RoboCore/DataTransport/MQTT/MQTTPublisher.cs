using System;
using System.Threading;
using System.Threading.Tasks;

using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using Serilog;

using RoboCore.Messages;

namespace RoboCore.DataTransport.MQTT
{
    public class MQTTPublisher<TMessage> : IPublisher<TMessage>, IDisposable where TMessage : class, new()
    {
        public string Topic { get; }
        
        private readonly CancellationTokenSource _cancellationToken;
        private readonly MqttQualityOfServiceLevel _qosLevel;
        private readonly IManagedMqttClient _client;
        private readonly MessageSerializer<PubSubMessage, TMessage> _serializer;

        public MQTTPublisher(MqttQualityOfServiceLevel qosLevel, IManagedMqttClient client, string topic)
        {
            _cancellationToken = new CancellationTokenSource();
            _serializer = new MessageSerializer<PubSubMessage, TMessage>();
            
            _qosLevel = qosLevel;
            _client = client ?? throw new ArgumentNullException(nameof(client));
            Topic = topic ?? throw new ArgumentNullException(nameof(topic));
        }

        public async Task PublishMessage(TMessage message, bool retained = false)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }
            
            var payload = _serializer.Serialize(message);
            var packedMessage = new MqttApplicationMessageBuilder()
                .WithTopic(Topic)
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
                Log.Error($"Failed to publish message on topic \"{Topic}\". Exception: {e}");
                throw;
            }
        }

        public void Dispose()
        {
            _cancellationToken.Cancel();
        }
    }
}