using System.Net;

using RoboCore.Config;

namespace RoboCore.DataTransport.MQTT.Discovery
{
    public class StaticDiscovery : IDiscovery
    {
        private readonly MQTTConfig _config;
        private readonly IPAddress _brokerIP;
        
        public StaticDiscovery(MQTTConfig config)
        {
            _config = config;
            if (_config.IsBroker)
            {
                return;
            }
            
            _brokerIP = IPAddress.Parse(_config.BrokerIPAddress);
        }

        public override void Start()
        {
            InvokeBrokerDiscovered(_config.NetworkID, _brokerIP, _config.BrokerPort);
        }

        public override void Stop() { }
    }
}