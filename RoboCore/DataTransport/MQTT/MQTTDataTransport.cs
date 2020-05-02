using System;
using System.Net;
using MQTTnet;

using RoboCore.Discovery;
using Serilog;

namespace RoboCore.DataTransport.MQTT
{
    public class MQTTDataTransport : IDataTransport
    {
        public delegate void BrokerDiscoveredHandler(string networkID, IPAddress address, int port);
        public event BrokerDiscoveredHandler HostDiscovered;
        
        private readonly bool _isBroker;
        private readonly string _networkID;
        private readonly MQTTBroker _broker;
        private readonly IDiscovery _discovery;

        private MqttFactory _mqttFactory;
        
        public MQTTDataTransport(bool isBroker, string networkID, IDiscovery discovery, MQTTBroker broker)
        {
            _isBroker = isBroker;
            _networkID = networkID;
            _broker = broker;
            
            _discovery = discovery ?? throw new ArgumentNullException(nameof(discovery));
            _discovery.HostDiscovered += OnHostDiscovered;

            _mqttFactory = new MqttFactory();
        }

        public void Start()
        {
            _discovery.Start();
        }

        public void Stop()
        {
            _discovery.Stop();
            _broker?.Stop();
        }
        
        //TODO(AFL): Move to AutoDiscovery
        private void OnHostDiscovered(string networkID, IPAddress address, int port)
        {
            if (!string.Equals(_networkID, networkID))
            {
                return;
            }
            
            if (!_isBroker)
            {
                InvokeBrokerDiscovered(networkID, address, port);
                return;
            }

            if (!_broker.IPAddress.Equals(address))
            {
                _discovery.PublishPresence();
                Log.Fatal($"More than one broker detected on RoboCore Network \"{_networkID}\".");
                Environment.Exit(-1);
            }
        }
        
        private void InvokeBrokerDiscovered(string networkID, IPAddress address, int port)
        {
            HostDiscovered?.Invoke(networkID, address, port);
        }
    }
}