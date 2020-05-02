using System;
using System.Net;
using RoboCore.Config;

namespace RoboCore.Discovery
{
    public class StaticDiscovery : IDiscovery
    {
        public event HostDiscoveredHandler HostDiscovered;

        private readonly bool _isBroker;
        private readonly IPAddress _ipAddress;
        private readonly string _networkID;
        private readonly int _port;

        public StaticDiscovery(RoboCoreConfig config)
        {
            _isBroker = config.IsBroker;
            if (_isBroker)
            {
                return;
            }
            
            var isIPValid = IPAddress.TryParse($"{config.BrokerIPAddress}", out _ipAddress);
            if (!isIPValid)
            {
                throw new ArgumentException("IP value provided is not a valid IP address");
            }
            
            var port = config.BrokerPort;
            if (port < 1)
            {
                throw new ArgumentException("Port value provided is not a valid IP address");
            }

            _port = port;
            _networkID = config.NetworkID;
        }

        public void PublishPresence()
        {
        }

        public void Start()
        {
            if (!_isBroker)
            {
                InvokeHostDiscovered(_ipAddress, _port);
            }
        }

        public void Stop() { }

        private void InvokeHostDiscovered(IPAddress address, int port)
        {
            HostDiscovered?.Invoke(_networkID, address, port);
        }
    }
}