using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Threading;
using MQTTnet.Client;
using Serilog;

using RoboCore.Config;
using RoboCore.DataTransport.MQTT.Discovery;

namespace RoboCore.DataTransport.MQTT
{
    public class MQTTDataTransport : IDataTransport
    {
        private readonly MQTTConfig _config;
        private MQTTBroker _broker;
        private IDiscovery _discovery;

        private bool _isRunning = false;
        private readonly AutoResetEvent _startWait;

        private readonly List<IDisposable> _publishers;

        private MqttClient _mqttClient;
        
        public MQTTDataTransport()
        {
            _config = new MQTTConfig();
            _startWait = new AutoResetEvent(false);
        }
        
        public override DataTransportConfigBase GetConfig()
        {
            return _config;
        }
        
        public override void Start()
        {
            if (_isRunning)
            {
                throw new InvalidOperationException("MQTT Data Transport is already running");
            }
            
            if (_config.IsBroker)
            {
                _broker = StartBroker(_config.BrokerPort);
            }

            _discovery = _config.UseAutoDiscovery ? (IDiscovery)new AutoDiscovery(_config) : new StaticDiscovery(_config);
            _discovery.BrokerDiscovered += (networkID, address, port) =>
            {
                InvokeBrokerDiscovered(networkID, address, port);
                _startWait.Set();

                if (!_isRunning)
                {
                    Log.Information($"MQTTDataTransport: MQTT Broker found.");
                    Log.Information($"MQTTDataTransport:     RoboCore Network: {networkID}");
                    Log.Information($"MQTTDataTransport:     Broker Address: {address}:{port}");
                }
            };
            Log.Information("MQTTDataTransport: Waiting to discover the MQTT Broker");
            _discovery.Start();

            while (!_startWait.WaitOne()) { }

            CreateMQTTClient();

            _isRunning = true;
        }
        
        public override void Stop()
        {
            if (!_isRunning)
            {
                return;
            }
            
            _discovery.Stop();
            _broker?.Stop();

            _isRunning = false;
        }

        private void CreateMQTTClient()
        {
            
        }
        
        public override ISubscriber<TMessage> CreateSubscriber<TMessage>(string topic)
        {
            //var publisher = new MQTTPublisher<TMessage>(_qosLevel, );
            throw new NotImplementedException();
        }

        public override IServiceEndpoint<TRequest, TResponse> CreateServiceEndpoint<TRequest, TResponse>(string topic)
        {
            throw new NotImplementedException();
        }

        public override IClientEndpoint<TRequest, TResponse> CreateClientEndpoint<TRequest, TResponse>(string topic)
        {
            throw new NotImplementedException();
        }

        public override IPublisher<TMessage> CreatePublisher<TMessage>(string topic)
        {
            throw new NotImplementedException();
        }

        #region Broker
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
        #endregion
    }
}