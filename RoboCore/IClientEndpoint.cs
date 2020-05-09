using System;

namespace RoboCore
{
    public interface IClientEndpoint<TRequest, TResponse> where TRequest : class, new() where TResponse : class, new()
    {
        public string RequestTopic { get; }
        public string ResponseTopic { get; }

        public void SendRequest(TRequest request, TimeSpan? requestTimeout, string destinationClientID = null, bool retained = false);
    }
}