using System.Net;

namespace RoboCore.DataTransport.MQTT.Discovery
{
    public abstract class IDiscovery
    {
        public event BrokerDiscoveredHandler BrokerDiscovered;

        public abstract void Start();
        public abstract void Stop();

        protected void InvokeBrokerDiscovered(string networkID, IPAddress address, int port)
        {
            BrokerDiscovered?.Invoke(networkID, address, port);
        }
    }
}