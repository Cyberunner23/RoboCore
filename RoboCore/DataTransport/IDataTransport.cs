using System;
using System.Net;

using RoboCore.Config;

namespace RoboCore.DataTransport
{
    public delegate void BrokerDiscoveredHandler(string networkID, IPAddress address, int port);
    
    public abstract class IDataTransport
    {
        public event BrokerDiscoveredHandler BrokerDiscovered;
        
        public abstract DataTransportConfigBase GetConfig();

        public TConfig GetConfig<TConfig>() where TConfig : DataTransportConfigBase
        {
            return (TConfig)GetConfig();
        }
        
        public abstract IPublisher<TMessage> CreatePublisher<TMessage>(string topic) where TMessage : class, new();
        public abstract ISubscriber CreateSubscriber<TMessage>(string topic, Action<TMessage> messageHandler) where TMessage : class, new();

        public abstract IServiceEndpoint CreateServiceEndpoint<TRequest, TResponse>(string topic, Func<TRequest, TResponse> requestReceivedHandler) 
            where TRequest : class, new() where TResponse : class, new();
        public abstract IClientEndpoint<TRequest, TResponse> CreateClientEndpoint<TRequest, TResponse>(string topic, Action<TResponse> responseReceivedHandler) 
            where TRequest : class, new() where TResponse : class, new();

        public abstract void Start();
        public abstract void Stop();

        protected void InvokeBrokerDiscovered(string networkID, IPAddress address, int port)
        {
            BrokerDiscovered?.Invoke(networkID, address, port);
        }
    }
}