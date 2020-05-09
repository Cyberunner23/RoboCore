using System;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;

using RoboCore.Messages;
using Serilog;

namespace RoboCore.DataTransport.MQTT
{
    public class MqttSubscriber<TMessage> : ISubscriber, IMessageHandler where TMessage : class, new()
    {
        public string Topic { get; }
        
        private readonly MessageSerializer<PubSubMessage, TMessage> _serializer;
        private readonly Action<TMessage> _messageReceivedHandler;

        public MqttSubscriber(IManagedMqttClient client, string topic, Action<TMessage> messageReceivedHandler)
        {
            _serializer = new MessageSerializer<PubSubMessage, TMessage>();
            _messageReceivedHandler = messageReceivedHandler;
            Topic = topic;
            
            try
            {
                client.SubscribeAsync(new TopicFilterBuilder().WithTopic(Topic).Build()).Wait();
            }
            catch (Exception e)
            {
                Log.Error($"Failed to subscribe to topic \"{topic}\": {e}");
                throw;
            }
        }
        
        public bool HandleMessageReceived(string serializedMessage)
        {
            TMessage message;
            var parseSucceeded = _serializer.Deserialize(serializedMessage, out message);
            _messageReceivedHandler(message);
            return parseSucceeded;
        }
    }
}