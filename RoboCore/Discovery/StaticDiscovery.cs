using System;
using System.Net;
using RoboCore.Config;

namespace RoboCore.Discovery
{
    public class StaticDiscovery : IDiscovery
    {
        private readonly IPAddress _ipAddress;

        private RoboCoreConfig _config;
        
        public StaticDiscovery(RoboCoreConfig config, bool isBroker) : this(config, null) { }

        public StaticDiscovery(RoboCoreConfig config, bool isBroker, string ip)
        {
            _config = config;
            
            var isValid = IPAddress.TryParse(ip, out _ipAddress);
            if (!isValid)
            {
                throw new ArgumentException("Value provided is not a valid IP address");
            }
        }
        
        public void Start(bool isBroker)
        {
            if (!isBroker && _ipAddress == null)
            {
                throw new InvalidOperationException("Cannot start without providing an IP address in the constructor");
            }

            if (isBroker)
            {
                
            }
            else
            {
                
            }
        }

        public void Stop()
        {
            throw new System.NotImplementedException();
        }
    }
}