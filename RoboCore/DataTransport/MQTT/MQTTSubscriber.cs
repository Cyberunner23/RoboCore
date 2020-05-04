using System;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;

using RoboCore.Messages;
using Serilog;

namespace RoboCore.DataTransport.MQTT
{
    public class MQTTSubscriber<TMessage> : ISubscriber<TMessage>
    {
        private readonly IManagedMqttClient _client;
        private readonly PubSubMessageSerializer<PubSubMessage, TMessage> _serializer;

        public MQTTSubscriber(IManagedMqttClient client, string topic)
        {
            _client = client;
            _serializer = new PubSubMessageSerializer<PubSubMessage, TMessage>();

            try
            {
                _client.SubscribeAsync(new TopicFilterBuilder().WithTopic(topic).Build()).Wait();
            }
            catch (Exception e)
            {
                Log.Error($"Failed to subscribe to topic \"{topic}\": {e}");
                throw;
            }
        }
        
        public override bool HandleMessageReceived(string serializedMessage)
        {
            TMessage message;
            var parseSucceeded = _serializer.Deserialize(serializedMessage, out message);
            InvokeMessageReceived(message);
            return parseSucceeded;
        }
    }
}