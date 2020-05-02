using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

using RoboCore.Config;
using Serilog;

namespace RoboCore.Discovery
{
    
    //TODO(AFL): Handle async/sync mess with start/stop and UDP locking reads
    public class AutoDiscovery : IDiscovery
    {
        private const string BroadcastIP = "255.255.255.255";
        
        public event HostDiscoveredHandler HostDiscovered;

        private readonly bool _isBroker;
        private readonly TimeSpan _broadcastInterval;
        private readonly int _broadcastPort;
        private readonly string _networkID;
        private readonly IPAddress _selfAddress;

        private CancellationTokenSource _cancellationToken;
        private UdpClient _listenerClient;

        public AutoDiscovery(RoboCoreConfig config)
        {
            _isBroker = config.IsBroker;
            _broadcastInterval = config.AutoDiscoveryBroadcastInterval;
            _broadcastPort = config.AutoDiscoveryBroadcastPort;
            _networkID = config.NetworkID;
            _selfAddress = IPUtils.GetLocalIP();
        }

        public void PublishPresence()
        {
            BroadcastAvailabilityMessage(_selfAddress, _broadcastPort);
        }

        public void Start()
        {
            _cancellationToken = new CancellationTokenSource();
            
            if (_isBroker)
            {
                StartBrokerBroadcast();
            }
            
            ListenBrokerBroadcast();
        }

        public void Stop()
        {
            _cancellationToken?.Cancel();
            _listenerClient.Close();
            _listenerClient.Dispose();
            _listenerClient = null;
        }

        private void ListenBrokerBroadcast()
        {
            var ipEndpoint = new IPEndPoint(IPAddress.Any, _broadcastPort);
            
            _listenerClient = new UdpClient();
            _listenerClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            _listenerClient.ExclusiveAddressUse = false;
            _listenerClient.Client.Bind(ipEndpoint);

            while (!_cancellationToken.IsCancellationRequested)
            {
                _listenerClient.BeginReceive(OnBroadcastReceived, null);    
            }
        }

        private void OnBroadcastReceived(IAsyncResult result)
        {
            var ipEndpoint = new IPEndPoint(IPAddress.Any, _broadcastPort);

            byte[] data = null;
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
            
            InvokeHostDiscovered(networkID, address, port);
        }
        
        private void StartBrokerBroadcast()
        {
            Task.Run(async () =>
            {
                while (!_cancellationToken.IsCancellationRequested)
                {
                    BroadcastAvailabilityMessage(_selfAddress, _broadcastPort);
                    await Task.Delay(_broadcastInterval);
                }
            });
        }
        
        

        private void BroadcastAvailabilityMessage(IPAddress address, int port)
        {
            var message = $"{_networkID}:{address}:{port}";
            var udpClient = new UdpClient();
            var ipEndpoint = new IPEndPoint(IPAddress.Parse(BroadcastIP), _broadcastPort);
            var messageBytes = Encoding.ASCII.GetBytes(message);
            
            udpClient.Client.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.ReuseAddress, true);
            udpClient.Client.Bind(ipEndpoint);
            udpClient.Send(messageBytes, messageBytes.Length, ipEndpoint);
            udpClient.Close();
        }

        private void InvokeHostDiscovered(string networkID, IPAddress address, int port)
        {
            HostDiscovered?.Invoke(networkID, address, port);
        }
    }
}