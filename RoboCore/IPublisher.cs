using System.Threading.Tasks;

namespace RoboCore
{
    public interface IPublisher<TMessage>
    {
        public string Topic { get; }

        public Task PublishMessage(TMessage message, bool retained = false);
    }
}