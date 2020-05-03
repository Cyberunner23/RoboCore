namespace RoboCore
{
    public interface IRoboCore
    {
        public bool Start();
        public void Stop();
        public IPublisher<TMessage> CreatePublisher<TMessage>(string topic);
        public ISubscriber<TMessage> CreateSubscriber<TMessage>(string topic);
        
        public IServiceEndpoint<TRequest, TResponse> CreateServiceEndpoint<TRequest, TResponse>(string topic);
        public IClientEndpoint<TRequest, TResponse> CreateClientEndpoint<TRequest, TResponse>(string topic);
    }
}