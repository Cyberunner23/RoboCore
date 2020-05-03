using System;
using System.Threading.Tasks;

namespace RoboCore
{
    public interface IPublisher<TMessage> : IDisposable
    {
        public Task PublishMessage(TMessage message, bool retained);
    }
}