
using RoboCore.Config;
using RoboCore.DataTransport;
using RoboCore.DataTransport.MQTT;

namespace RoboCore
{
    /// <summary>
    /// Main object for using RoboCore, sets up publishers and subscribers and runs the whole operation
    /// </summary>
    public class RoboCore : IRoboCore
    {
        public static readonly RoboCoreConfig DefaultConfig = new RoboCoreConfig();

        private readonly RoboCoreConfig _config;
        private readonly IDataTransport _dataTransport;

        public RoboCore() : this(DefaultConfig) { }

        public RoboCore(RoboCoreConfig config)
        {
            _config = config;
            _config.Validate();
            
            var brokerTransportBootstrapper = new MQTTDataTransportBootstrapper(_config);
            _dataTransport = brokerTransportBootstrapper.CreateMQTTDataTransport();
        }

        public bool Start()
        {
            return false;
        }

        public void Stop()
        {
            
        }

        public void CreatePublisher<MessageType>(string topic)
        {
        }

        public void CreateSubscriber<MessageType>(string topic)
        {
        }

        public void CreateServiceEndpoint<RequestType, ResponseType>(string topic)
        {
        }

        public void CreateClientEndpoint<RequestType, ResponseType>(string topic)
        {
        }
    }
}