using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using Serilog;

using RoboCore.Config;

namespace RoboCore.DataTransport.MQTT.Discovery
{
    public class AutoDiscovery : IDiscovery
    {
        private const string BroadcastIP = "255.255.255.255";

        private readonly MQTTConfig _config;
        private readonly IPAddress _selfAddress;

        private CancellationTokenSource _cancellationToken;
        private UdpClient _listenerClient;

        public AutoDiscovery(MQTTConfig config)
        {
            _config = config;
            _selfAddress = IPUtils.GetLocalIP();
        }

        public override void Start()
        {
            _cancellationToken = new CancellationTokenSource();
            
            if (_config.IsBroker)
            {
                StartBrokerBroadcast();
            }
            
            ListenToBrokerBroadcast();
        }

        public override void Stop()
        {
            _cancellationToken?.Cancel();
            _listenerClient.Close();
        }

        private void StartBrokerBroadcast()
        {
            Task.Run(async () =>
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    BroadcastAvailabilityMessage(_selfAddress, _config.AutoDiscoveryBroadcastPort);
                    await Task.Delay(_config.AutoDiscoveryBroadcastInterval);
                }
            });
        }
        
        private void ListenToBrokerBroadcast()
        {
            var ipEndpoint = new IPEndPoint(IPAddress.Any, _config.AutoDiscoveryBroadcastPort);
            
            _listenerClient = new UdpClient();
            _listenerClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _listenerClient.ExclusiveAddressUse = false;
            _listenerClient.Client.Bind(ipEndpoint);

            BeginBroadcastReceive();
        }

        private void BeginBroadcastReceive()
        {
            _listenerClient.BeginReceive(OnBroadcastReceived, null);
        }

        private void OnBroadcastReceived(IAsyncResult result)
        {
            var ipEndpoint = new IPEndPoint(IPAddress.Any, _config.AutoDiscoveryBroadcastPort);

            byte[] data;
            try
            {
                data = _listenerClient.EndReceive(result, ref ipEndpoint);
            }
            catch (ObjectDisposedException)
            {
                return;
            }
            
            var message = Encoding.UTF8.GetString(data); 
            var components = message.Split(':');

            if (components.Length != 3)
            {
                Log.Warning($"Received badly formatted broadcast message: {message}");
                return;
            }

            string networkID = components[0];
            IPAddress address;
            int port;

            if (!IPAddress.TryParse(components[1], out address))
            {
                Log.Warning($"Received badly formatted broadcast message (bad IP): {message}");
                return;
            }

            if (!int.TryParse(components[2], out port))
            {
                Log.Warning($"Received badly formatted broadcast message (bad Port): {message}");
                return;
            }

            if (_config.IsBroker)
            {
                VerifyHost(networkID, address);    
            }
            
            InvokeBrokerDiscovered(networkID, address, port);

            if (!_cancellationToken.IsCancellationRequested)
            {
                BeginBroadcastReceive();
            }
        }
        
        private void VerifyHost(string networkID, IPAddress address)
        {
            if (!string.Equals(_config.NetworkID, networkID))
            {
                return;
            }

            if (!_selfAddress.Equals(address))
            {
                PublishPresence();
                Log.Fatal($"More than one broker detected on RoboCore Network \"{_config.NetworkID}\".");
                Environment.Exit(-1);
            }
        }

        #region Broker Broadcasting
        private void PublishPresence()
        {
            BroadcastAvailabilityMessage(_selfAddress, _config.AutoDiscoveryBroadcastPort);
        }

        private void BroadcastAvailabilityMessage(IPAddress address, int port)
        {
            var message = $"{_config.NetworkID}:{address}:{port}";
            var udpClient = new UdpClient();
            var ipEndpoint = new IPEndPoint(IPAddress.Parse(BroadcastIP), _config.AutoDiscoveryBroadcastPort);
            var messageBytes = Encoding.ASCII.GetBytes(message);
            
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(ipEndpoint);
            udpClient.Send(messageBytes, messageBytes.Length, ipEndpoint);
            udpClient.Close();
        }
        #endregion
    }
}