using System;
using System.Linq;
using System.Net.NetworkInformation;

using RoboCore.Config;
using RoboCore.Discovery;

namespace RoboCore.DataTransport.MQTT
{
    public class MQTTDataTransportBootstrapper
    {
        private readonly RoboCoreConfig _config;
        
        public MQTTDataTransportBootstrapper(RoboCoreConfig config)
        {
            _config = config;
        }
        
        public IDataTransport CreateMQTTDataTransport()
        {
            var isBroker = _config.IsBroker;
            MQTTBroker broker = null;
            if (isBroker)
            {
                broker = StartBroker(_config.BrokerPort);
            }

            var networkID = _config.NetworkID;
            var discovery = _config.UseAutoDiscovery ? (IDiscovery)new AutoDiscovery(_config) : new StaticDiscovery(_config);
            var transport = new MQTTDataTransport(isBroker, networkID, discovery, broker);
            return transport;
        }

        private MQTTBroker StartBroker(int port)
        {
            return port == -1 ? StartBrokerAutoFindPort() : StartBrokerWithPort(port);
        }

        private MQTTBroker StartBrokerWithPort(int port)
        {
            var broker = new MQTTBroker(port);
            broker.Start();
            return broker;
        }

        private MQTTBroker StartBrokerAutoFindPort()
        {
            var port = MQTTBroker.DefaultPort - 1;
            MQTTBroker broker = null;

            var brokerStarted = false;
            do
            {
                ++port;
                
                if (!IsPortAvailable(port))
                {
                    continue;
                }

                try
                {
                    broker = new MQTTBroker(port);
                    broker.Start();
                    brokerStarted = true;
                }
                catch (Exception)
                {
                    continue;
                }
                
            } while (!brokerStarted);

            return broker;
        }
        
        private bool IsPortAvailable(int port)
        {
            var ipGlobalProperties = IPGlobalProperties.GetIPGlobalProperties();
            var tcpConnectionInfos = ipGlobalProperties.GetActiveTcpConnections();
            var portInUse = tcpConnectionInfos.Select(x => x.LocalEndPoint.Port).Contains(port);

            return !portInUse;
        }
    }
}