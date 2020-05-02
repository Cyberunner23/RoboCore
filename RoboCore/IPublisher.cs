namespace RoboCore
{
    public interface IPublisher<TMessage>
    {
        public void PublishMessage(TMessage message);
    }
}