using System;
using RoboCore.Discovery;

namespace RoboCore.Config
{
    public class RoboCoreConfig
    {
        /// <summary>
        /// Whether this instance of RoboCore acts as the MQTT Broker.
        /// NOTE: Only one MQTT Broker may be present on a RoboCore network at any given time.
        ///       This is specially important when using AutoDiscovery
        /// </summary>
        public bool IsBroker { get; set; } = false;

        /// <summary>
        /// ID of the RoboCore network.
        /// Multiple RoboCore networks can be present on a given LAN network.
        /// </summary>
        public string NetworkID { get; set; } = "default";

        /// <summary>
        /// Time delay between sending heartbeats to the other Robot.
        /// </summary>
        public TimeSpan HeartbeatTimeout { get; set; } = TimeSpan.FromSeconds(1);

        /// <summary>
        /// How many heartbeats we miss until we consider the other Robot to be gone.
        /// Value must be >= 2
        /// </summary>
        private const int MinHeartbeatMissCount = 2;
        private int _heartbeatMissCount = MinHeartbeatMissCount;
        public int HeartbeatMissCount { get { return _heartbeatMissCount; } set { _heartbeatMissCount = value < MinHeartbeatMissCount ? MinHeartbeatMissCount : value; } }
    }
}