
using RoboCore.Config;
using RoboCore.Messages;

namespace RoboCore
{
    /// <summary>
    /// Main object for using RoboCore, sets up publishers and subscribers and runs the whole operation
    /// </summary>
    public class RoboCore : IRoboCore
    {
        public static readonly RoboCoreConfig DefaultConfig = new RoboCoreConfig();
        public static readonly RoboCoreComponents DefaultComponents = new RoboCoreComponents(DefaultConfig);
        
        private readonly RoboCoreConfig _config;
        private readonly RoboCoreComponents _components;

        public RoboCore() : this(DefaultConfig, DefaultComponents) { }
        public RoboCore(RoboCoreConfig config) : this(config, new RoboCoreComponents(config)) { }

        public RoboCore(RoboCoreConfig config, RoboCoreComponents components)
        {
            _config = config;
            _components = components;
        }

        public bool Init()
        {
            return false;
        }

        public void Shutdown()
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