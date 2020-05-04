namespace RoboCore
{
    public interface ISubscriber
    {
        public bool HandleMessageReceived(string serializedMessage);
    }
    
    public abstract class ISubscriber<TMessage> : ISubscriber
    {
        public delegate void MessageReceivedHandler(TMessage message);
        public event MessageReceivedHandler MessageReceived;
        
        public abstract bool HandleMessageReceived(string serializedMessage);

        protected void InvokeMessageReceived(TMessage message)
        {
            MessageReceived?.Invoke(message);
        }
    }
}