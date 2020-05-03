using System;
using System.Net;
using MQTTnet.Protocol;
using RoboCore.DataTransport;
using RoboCore.DataTransport.MQTT;
using RoboCore.Exceptions;

namespace RoboCore.Config
{
    public class RoboCoreConfig : LockingConfig
    {
        /// <summary>
        /// ID of the RoboCore Network.
        /// Multiple RoboCore Networks can be present on a given LAN network.
        /// </summary>
        private string _networkID = "default";

        public string NetworkID
        {
            get => _networkID;
            set
            {
                ThrowIfLocked();
                _networkID = value;
            }
        }

        /// <summary>
        /// The Data transport to be used, currently only MQTT is available, more may be added in the future.
        /// </summary>
        public IDataTransport DataTransport { get; set; } = new MQTTDataTransport();

        public void Validate()
        {
            if (string.IsNullOrEmpty(NetworkID))
            {
                throw new ConfigurationException($"{nameof(NetworkID)} must have a value");
            }

            Lock();

            if (DataTransport == null)
            {
                throw new ConfigurationException($"{nameof(DataTransport)} must have a value");
            }

            DataTransport.GetConfig().NetworkID = NetworkID; // FIXME(AFL): Kinda hacky
            DataTransport.GetConfig().Validate();
        }
    }
}