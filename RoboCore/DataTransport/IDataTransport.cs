using System.Net;
using RoboCore.Config;
using RoboCore.DataTransport.MQTT.Discovery;

namespace RoboCore.DataTransport
{
    public abstract class IDataTransport
    {
        public event BrokerDiscoveredHandler BrokerDiscovered;
        
        public abstract DataTransportConfigBase GetConfig();

        public TConfig GetConfig<TConfig>() where TConfig : DataTransportConfigBase
        {
            return (TConfig)GetConfig();
        }
        
        public abstract IPublisher<TMessage> CreatePublisher<TMessage>(string topic);
        public abstract ISubscriber<TMessage> CreateSubscriber<TMessage>(string topic);
        
        public abstract IServiceEndpoint<TRequest, TResponse> CreateServiceEndpoint<TRequest, TResponse>(string topic);
        public abstract IClientEndpoint<TRequest, TResponse> CreateClientEndpoint<TRequest, TResponse>(string topic);
        
        public abstract void Start();
        public abstract void Stop();

        protected void InvokeBrokerDiscovered(string networkID, IPAddress address, int port)
        {
            BrokerDiscovered?.Invoke(networkID, address, port);
        }
    }
}