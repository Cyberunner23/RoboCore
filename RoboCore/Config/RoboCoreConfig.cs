using System;
using System.Net;
using RoboCore.Exceptions;

namespace RoboCore.Config
{
    public class RoboCoreConfig
    {
        /// <summary>
        /// ID of the RoboCore Network.
        /// Multiple RoboCore Networks can be present on a given LAN network.
        /// </summary>
        public string NetworkID { get; set; } = "default";
        
        /// <summary>
        /// Whether this instance of RoboCore acts as the MQTT Broker.
        /// NOTE: Only one MQTT Broker may be present on a RoboCore network at any given time.
        ///       This is specially important when using Auto Discovery.
        /// </summary>
        public bool IsBroker { get; set; } = false;

        /// <summary>
        /// Whether this instance of RoboCore will use Auto Discovery.
        /// When in use, the Robots that aren't the Broker receive a broadcast with the IP and port of the broker.
        /// If required, the IP and port of the broker can be manually set by setting <see cref="BrokerIPAddress"/> and <see cref="BrokerPort"/>
        /// NOTE: This value must be the same for all Robots on the RoboCore Network.
        /// </summary>
        public bool UseAutoDiscovery { get; set; } = true;
        
        /// <summary>
        /// Interval of time between broadcasts of Broker presence.
        /// NOTE: Only used when <see cref="IsBroker"/> is true and <see cref="UseAutoDiscovery"/> is true.
        /// </summary>
        public TimeSpan AutoDiscoveryBroadcastInterval { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// Port used when broadcasting the presence of the Broker.
        /// NOTE: Only used when <see cref="IsBroker"/> is true and <see cref="UseAutoDiscovery"/> is true.
        /// </summary>
        public int AutoDiscoveryBroadcastPort { get; set; } = 1882;
        
        /// <summary>
        /// IP Address (without port) of the broker.
        /// NOTE: Only required when <see cref="IsBroker"/> is false and <see cref="UseAutoDiscovery"/> is false
        /// NOTE: This value must be the same for all Robots on the RoboCore Network.
        /// </summary>
        public string BrokerIPAddress { get; set; }

        /// <summary>
        /// Port of the broker.
        /// NOTE: Only required when <see cref="UseAutoDiscovery"/> is false
        /// NOTE: This value must be the same for all Robots on the RoboCore Network.
        /// </summary>
        public int BrokerPort { get; set; } = -1;

        /// <summary>
        /// Time delay between sending heartbeats to the other Robot.
        /// </summary>
        public TimeSpan HeartbeatTimeout { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// How many heartbeats we miss until we consider the other Robot to be gone.
        /// NOTE: Value must be >= 2
        /// </summary>
        private const int MinHeartbeatMissCount = 2;
        private int _heartbeatMissCount = MinHeartbeatMissCount;
        public int HeartbeatMissCount { get { return _heartbeatMissCount; } set { _heartbeatMissCount = value < MinHeartbeatMissCount ? MinHeartbeatMissCount : value; } }

        public void Validate()
        {
            if (!UseAutoDiscovery && BrokerPort < 1)
            {
                throw new ConfigurationException("BrokerPort must be set");
            }

            if (!IsBroker && !UseAutoDiscovery && string.IsNullOrEmpty(BrokerIPAddress))
            {
                throw new ConfigurationException("BrokerIPAddress must be set");
            }

            if (!string.IsNullOrEmpty(BrokerIPAddress))
            {
                var isValid = IPAddress.TryParse(BrokerIPAddress, out _);
                if (!isValid)
                {
                    throw new ConfigurationException("Invalid value for BrokerIPAddress provided");
                }
            }

            if (AutoDiscoveryBroadcastPort < 1)
            {
                throw new ConfigurationException("Invalid value for AutoDiscoveryBroadcastPort provided");
            }
        }
    }
}