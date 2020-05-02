namespace RoboCore
{
    public interface IRoboCore
    {
        public bool Init();
        public void Shutdown();
        public void CreatePublisher<MessageType>(string topic);
        public void CreateSubscriber<MessageType>(string topic);
        
        public void CreateServiceEndpoint<RequestType, ResponseType>(string topic);
        public void CreateClientEndpoint<RequestType, ResponseType>(string topic);
    }
}