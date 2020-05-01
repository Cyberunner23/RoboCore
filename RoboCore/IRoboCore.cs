using RoboCore.Messages;

namespace RoboCore
{
    public interface IRoboCore
    {
        public bool Init();
        public void CreatePublisher<MessageType>(string topic) where MessageType : MessageBase;
        public void CreateSubscriber<MessageType>(string topic) where MessageType : MessageBase;
        
        public void CreateServiceEndpoint<RequestType, ResponseType>(string topic) where RequestType : MessageBase where ResponseType : MessageBase;
        public void CreateClientEndpoint<RequestType, ResponseType>(string topic) where RequestType : MessageBase where ResponseType : MessageBase;
    }
}