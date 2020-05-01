
using RoboCore.Config;
using RoboCore.Messages;

namespace RoboCore
{
    /// <summary>
    /// Main object for using RoboCore, sets up publishers and subscribers and runs the whole operation
    /// </summary>
    public class RoboCore : IRoboCore
    {
        private static readonly RoboCoreConfig DefaultConfig = new RoboCoreConfig();
        private static readonly RoboCoreComponents DefaultComponents = new RoboCoreComponents(DefaultConfig);
        
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

        public void CreatePublisher<MessageType>(string topic) where MessageType : MessageBase
        {
        }

        public void CreateSubscriber<MessageType>(string topic) where MessageType : MessageBase
        {
        }

        public void CreateServiceEndpoint<RequestType, ResponseType>(string topic) where RequestType : MessageBase where ResponseType : MessageBase
        {
        }

        public void CreateClientEndpoint<RequestType, ResponseType>(string topic) where RequestType : MessageBase where ResponseType : MessageBase
        {
        }
    }
}