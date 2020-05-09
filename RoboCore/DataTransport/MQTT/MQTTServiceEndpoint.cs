using System;
using System.Threading;
using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using Serilog;

using RoboCore.Messages;

namespace RoboCore.DataTransport.MQTT
{
    public class MQTTServiceEndpoint<TRequest, TResponse> : IServiceEndpoint, IMessageHandler, IDisposable where TRequest : class, new() where TResponse : class, new()
    {
        public string RequestTopic { get; }
        public string ResponseTopic { get; }

        private readonly CancellationTokenSource _cancellationToken;
        private readonly MqttQualityOfServiceLevel _qosLevel;
        private readonly IManagedMqttClient _client;
        private readonly MessageSerializer<EndpointMessage, TRequest> _requestDeserializer;
        private readonly MessageSerializer<EndpointMessage, TResponse> _responseSerializer;
        private readonly Func<TRequest, TResponse> _requestReceivedHandler;
        
        public MQTTServiceEndpoint(MqttQualityOfServiceLevel qosLevel,IManagedMqttClient client, string topic, Func<TRequest, TResponse> requestReceivedHandler)
        {
            _cancellationToken = new CancellationTokenSource();
            _qosLevel = qosLevel;
            _client = client;
            _requestDeserializer = new MessageSerializer<EndpointMessage, TRequest>();
            _responseSerializer = new MessageSerializer<EndpointMessage, TResponse>();
            _requestReceivedHandler = requestReceivedHandler;

            RequestTopic = $"__{topic}_Request__";
            ResponseTopic = $"__{topic}_Response__";
            
            try
            {
                _client.SubscribeAsync(new TopicFilterBuilder().WithTopic(RequestTopic).Build()).Wait();
            }
            catch (Exception e)
            {
                Log.Error($"Failed to subscribe to request topic \"{RequestTopic}\" for topic \"{topic}\": {e}");
                throw;
            }
        }


        public bool HandleMessageReceived(string serializedMessage)
        {
            EndpointMessage requestMessage;
            if (!_requestDeserializer.Deserialize(serializedMessage, out requestMessage))
            {
                return false;
            }

            var requestSource = requestMessage.Source;
            var requestDestination = requestMessage.Destination;

            // Ignore request if its meant for another bot
            if (!string.IsNullOrEmpty(requestDestination) && !string.Equals(requestDestination, _client.Options.ClientOptions.ClientId))
            {
                return true;
            }

            TRequest request;
            if (!_requestDeserializer.Deserialize(requestMessage, out request))
            {
                return false;
            }

            var response = _requestReceivedHandler(request);
            var responseMessage = _responseSerializer.SerializeMessage(response);
            responseMessage.Source = requestDestination;
            responseMessage.Destination = requestSource;

            var payload = _responseSerializer.SerializeWrapper(responseMessage);
            var packedMessage = new MqttApplicationMessageBuilder()
                .WithTopic(ResponseTopic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(_qosLevel)
                .Build();
            
            try
            {
                _client.PublishAsync(packedMessage, _cancellationToken.Token).Wait();
            }
            catch (Exception e)
            {
                Log.Error($"Failed to publish response message on topic \"{ResponseTopic}\". Exception: {e}");
                throw;
            }
            
            return true;
        }

        public void Dispose()
        {
            _cancellationToken.Cancel();
        }
    }
}
