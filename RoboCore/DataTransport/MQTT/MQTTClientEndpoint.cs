using System;
using System.Threading;

using MQTTnet;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Protocol;
using Serilog;

using RoboCore.Messages;

namespace RoboCore.DataTransport.MQTT
{
    
    // Rebuild with callback
    public class MQTTClientEndpoint<TRequest, TResponse> : IClientEndpoint<TRequest, TResponse>, IMessageHandler
        where TRequest : class, new() where TResponse : class, new()
    {
        public string RequestTopic { get; }
        public string ResponseTopic { get; }
        
        private readonly CancellationTokenSource _cancellationToken;
        private readonly MqttQualityOfServiceLevel _qosLevel;
        private readonly IManagedMqttClient _client;
        private readonly Action<TResponse> _responseRceivedHandler;
        
        private readonly AutoResetEvent _responseWait;
        private readonly MessageSerializer<EndpointMessage, TRequest> _requestSerializer;
        private readonly MessageSerializer<EndpointMessage, TResponse> _responseDeserializer;

        public MQTTClientEndpoint(MqttQualityOfServiceLevel qosLevel, IManagedMqttClient client, string topic, Action<TResponse> responseReceivedHandler)
        {
            _cancellationToken = new CancellationTokenSource();
            _responseWait = new AutoResetEvent(false);
            _qosLevel = qosLevel;
            _client = client;
            _responseRceivedHandler = responseReceivedHandler;
            
            _requestSerializer = new MessageSerializer<EndpointMessage, TRequest>();
            _responseDeserializer = new MessageSerializer<EndpointMessage, TResponse>();
            
            RequestTopic = $"__{topic}_Request__";
            ResponseTopic = $"__{topic}_Response__";
            
            try
            {
                _client.SubscribeAsync(new TopicFilterBuilder().WithTopic(ResponseTopic).Build()).Wait();
            }
            catch (Exception e)
            {
                Log.Error($"Failed to subscribe to request topic \"{ResponseTopic}\" for topic \"{topic}\": {e}");
                throw;
            }
        }

        // FIXME?(AFL): kinda ugly signature 
        public void SendRequest(TRequest request, TimeSpan? requestTimeout, string destinationClientID = null, bool retained = false)
        {
            requestTimeout = requestTimeout ?? TimeSpan.FromSeconds(20);
            
            var wrappedRequest = _requestSerializer.SerializeMessage(request);
            wrappedRequest.Source = _client.Options.ClientOptions.ClientId;;
            wrappedRequest.Destination = destinationClientID;

            var payload = _requestSerializer.SerializeWrapper(wrappedRequest);
            var packedMessage = new MqttApplicationMessageBuilder()
                .WithTopic(RequestTopic)
                .WithPayload(payload)
                .WithQualityOfServiceLevel(_qosLevel)
                .WithRetainFlag(retained)
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
        }

        public bool HandleMessageReceived(string serializedMessage)
        {
            EndpointMessage responseMessage;
            // TODO(AFL): error handling
            if (!_responseDeserializer.Deserialize(serializedMessage, out responseMessage))
            {
                return false;
            }

            if (!string.IsNullOrEmpty(responseMessage.Destination) 
                && !string.Equals(responseMessage.Destination, _client.Options.ClientOptions.ClientId))
            {
                return true;
            }

            // TODO(AFL): error handling
            TResponse response;
            if (!_responseDeserializer.Deserialize(responseMessage, out response))
            {
                return false;
            }

            _responseRceivedHandler(response);
            return true;
        }
    }
}