using System;
using System.Collections.Generic;
using System.Linq;
using System.Net;
using System.Net.NetworkInformation;
using System.Text;
using System.Threading;

using MQTTnet;
using MQTTnet.Client.Connecting;
using MQTTnet.Client.Disconnecting;
using MQTTnet.Client.Options;
using MQTTnet.Client.Receiving;
using MQTTnet.Extensions.ManagedClient;
using MQTTnet.Formatter;
using Serilog;

using RoboCore.Config;
using RoboCore.DataTransport.MQTT.Discovery;

namespace RoboCore.DataTransport.MQTT
{
    public class MQTTDataTransport : IDataTransport
    {
        public delegate void ClientConnectedHandler(MqttClientConnectedEventArgs args);
        public delegate void ClientDisconnectedHandler(MqttClientDisconnectedEventArgs args);

        public event ClientConnectedHandler ClientConnected;
        public event ClientDisconnectedHandler ClientDisconnected;
        
        private readonly MQTTConfig _config;
        private MQTTBroker _broker;
        
        private IDiscovery _discovery;
        private IPAddress _brokerIP;
        private int _brokerPort;

        private bool _isRunning = false;
        private readonly AutoResetEvent _startWait;
        private readonly AutoResetEvent _connectWait;

        private readonly Dictionary<string, IMessageHandler> _subscribers;
        private readonly List<IDisposable> _publishers;

        private IManagedMqttClient _mqttClient;
        
        public MQTTDataTransport()
        {
            _config = new MQTTConfig();
            _startWait = new AutoResetEvent(false);
            _connectWait = new AutoResetEvent(false);
            
            _subscribers = new Dictionary<string, IMessageHandler>();
            _publishers = new List<IDisposable>();
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
                _brokerIP = address;
                _brokerPort = port;
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

            Log.Information("MQTTDataTransport: Connecting to Broker...");
            CreateMQTTClient();
            
            while (!_connectWait.WaitOne()) { }
            
            Log.Information("MQTTDataTransport: Connected to Broker");

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
        
        public override IPublisher<TMessage> CreatePublisher<TMessage>(string topic)
        {
            var publisher = new MQTTPublisher<TMessage>(_config.QOSLevel, _mqttClient, topic);
            //_publishers.Add(publisher);
            return publisher;
        }
        
        public override ISubscriber CreateSubscriber<TMessage>(string topic, Action<TMessage> messageHandler)
        {
            var subscriber = new MqttSubscriber<TMessage>(_mqttClient, topic, messageHandler);
            _subscribers.Add(topic, subscriber);
            return subscriber;
        }

        public override IServiceEndpoint CreateServiceEndpoint<TRequest, TResponse>(string topic, Func<TRequest, TResponse> requestReceivedHandler)
        {
            var serviceEndpoint = new MQTTServiceEndpoint<TRequest, TResponse>(_config.QOSLevel, _mqttClient, topic, requestReceivedHandler);
            _subscribers.Add(serviceEndpoint.RequestTopic, serviceEndpoint);
            return serviceEndpoint;
        }
        
        public override IClientEndpoint<TRequest, TResponse> CreateClientEndpoint<TRequest, TResponse>(string topic, Action<TResponse> responseReceivedHandler)
        {
            var clientEndpoint = new MQTTClientEndpoint<TRequest, TResponse>(_config.QOSLevel, _mqttClient, topic, responseReceivedHandler);
            _subscribers.Add(clientEndpoint.ResponseTopic, clientEndpoint);
            return clientEndpoint;
        }

        private void CreateMQTTClient()
        {
            var clientOptions = new MqttClientOptions()
            {
                ClientId = _config.RobotName,
                ProtocolVersion = MqttProtocolVersion.V500,
                ChannelOptions = new MqttClientTcpOptions()
                {
                    Server = $"{_brokerIP}",
                    Port = _brokerPort,
                    TlsOptions = _config.TLSOptions
                },
                KeepAlivePeriod = _config.KeepalivePeriod,
                CleanSession = _config.CleanSession
            };
            
            _mqttClient = new MqttFactory().CreateManagedMqttClient();
            _mqttClient.ConnectedHandler = new MqttClientConnectedHandlerDelegate(OnConnected);
            _mqttClient.DisconnectedHandler = new MqttClientDisconnectedHandlerDelegate(OnDisconnected);
            _mqttClient.ApplicationMessageReceivedHandler = new MqttApplicationMessageReceivedHandlerDelegate(OnMessageReceived);

            _mqttClient.StartAsync(new ManagedMqttClientOptions()
            {
                ClientOptions = clientOptions
            }).Wait();
        }

        private void OnConnected(MqttClientConnectedEventArgs args)
        {
            _connectWait.Set();
            InvokeClientConnected(args);
        }
        
        private void OnDisconnected(MqttClientDisconnectedEventArgs args)
        {
            InvokeClientDisconnected(args);
        }
        
        private void OnMessageReceived(MqttApplicationMessageReceivedEventArgs args)
        {
            var topic = args.ApplicationMessage.Topic;
            IMessageHandler subscriber;
            if (!_subscribers.TryGetValue(topic, out subscriber))
            {
                Log.Debug("MQTTDataTransport: Received message for unsubscribed topic");
                return;
            }

            var messageStr = Encoding.UTF8.GetString(args.ApplicationMessage.Payload);
            args.ProcessingFailed = !subscriber.HandleMessageReceived(messageStr);
        }
        
        private void InvokeClientConnected(MqttClientConnectedEventArgs args)
        {
            ClientConnected?.Invoke(args);
        }

        private void InvokeClientDisconnected(MqttClientDisconnectedEventArgs args)
        {
            ClientDisconnected?.Invoke(args);
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