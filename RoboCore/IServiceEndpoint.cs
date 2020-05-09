namespace RoboCore
{
    public interface IServiceEndpoint
    {
        public string RequestTopic { get; }
        public string ResponseTopic { get; }
    }
}