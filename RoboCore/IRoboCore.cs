using System;

namespace RoboCore
{
    public interface IRoboCore
    {
        public bool Start();
        public void Stop();
        public IPublisher<TMessage> CreatePublisher<TMessage>(string topic) where TMessage : class, new();
        public ISubscriber CreateSubscriber<TMessage>(string topic, Action<TMessage> messageReceivedHandler) where TMessage : class, new();
        
        public IServiceEndpoint CreateServiceEndpoint<TRequest, TResponse>(string topic, Func<TRequest, TResponse> requestReceivedHandler)
            where TRequest : class, new() where TResponse : class, new();
        public IClientEndpoint<TRequest, TResponse> CreateClientEndpoint<TRequest, TResponse>(string topic, Action<TResponse> responseReceivedHandler)
            where TRequest : class, new() where TResponse : class, new();
    }
}