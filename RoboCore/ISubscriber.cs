namespace RoboCore
{
    
    public interface ISubscriber<T>
    {
        public delegate void MessageReceivedHandler(T message);
        public event MessageReceivedHandler MessageReceived;
    }
}