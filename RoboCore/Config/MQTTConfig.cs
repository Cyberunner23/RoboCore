using System;
using System.Net;
using MQTTnet.Client.Options;
using MQTTnet.Protocol;
using RoboCore.DataTransport.MQTT.Discovery;
using RoboCore.Exceptions;

namespace RoboCore.Config
{
    public class MQTTConfig : DataTransportConfigBase
    {
        /// <summary>
        /// Whether this instance of RoboCore acts as the MQTT Broker.
        /// NOTE: Only one MQTT Broker may be present on a RoboCore network at any given time.
        ///       This is specially important when using Auto Discovery.
        /// </summary>
        private bool _isBroker = false;
        public bool IsBroker
        {
            get => _isBroker;
            set
            {
                ThrowIfLocked();
                _isBroker = value;
            }
        }

        /// <summary>
        /// Name of the robot, used to differentiate between robots.
        /// </summary>
        private string _robotName = $"{IPUtils.GetLocalIP()}";
        public string RobotName
        {
            get => _robotName;
            set
            {
                ThrowIfLocked();
                _robotName = value;
            }
        }

        /// <summary>
        /// Whether this instance of RoboCore will use Auto Discovery.
        /// When in use, the Robots that aren't the Broker receive a broadcast with the IP and port of the broker.
        /// If required, the IP and port of the broker can be manually set by setting <see cref="BrokerIPAddress"/> and <see cref="BrokerPort"/>
        /// NOTE: This value must be the same for all Robots on the RoboCore Network.
        /// </summary>
        private bool _useAutoDiscovery = true;
        public bool UseAutoDiscovery
        {
            get => _useAutoDiscovery;
            set
            {
                ThrowIfLocked();
                _isBroker = value;
            }
        }

        /// <summary>
        /// Interval of time between broadcasts of Broker presence.
        /// NOTE: Only used when <see cref="IsBroker"/> is true and <see cref="UseAutoDiscovery"/> is true.
        /// </summary>
        private TimeSpan _autoDiscoveryBroadcastInterval = TimeSpan.FromSeconds(1);
        public TimeSpan AutoDiscoveryBroadcastInterval
        {
            get => _autoDiscoveryBroadcastInterval;
            set
            {
                ThrowIfLocked();
                _autoDiscoveryBroadcastInterval = value;
            }
        }

        /// <summary>
        /// Port used when broadcasting the presence of the Broker.
        /// NOTE: Only used when <see cref="IsBroker"/> is true and <see cref="UseAutoDiscovery"/> is true.
        /// </summary>
        private int _autoDiscoveryBroadcastPort = 1882;
        public int AutoDiscoveryBroadcastPort
        {
            get => _autoDiscoveryBroadcastPort;
            set
            {
                ThrowIfLocked();
                _autoDiscoveryBroadcastPort = value;
            }
        }

        /// <summary>
        /// IP Address (without port) of the broker.
        /// NOTE: Only required when <see cref="IsBroker"/> is false and <see cref="UseAutoDiscovery"/> is false
        /// NOTE: This value must be the same for all Robots on the RoboCore Network.
        /// </summary>
        private string _brokerIPAddress;
        public string BrokerIPAddress
        {
            get => _brokerIPAddress;
            set
            {
                ThrowIfLocked();
                _brokerIPAddress = value;
            }
        }

        /// <summary>
        /// Port of the broker.
        /// NOTE: Only required when <see cref="UseAutoDiscovery"/> is false
        /// NOTE: This value must be the same for all Robots on the RoboCore Network.
        /// </summary>
        private int _brokerPort = 1882;
        public int BrokerPort
        {
            get => _brokerPort;
            set
            {
                ThrowIfLocked();
                _brokerPort = value;
            }
        }

        /// <summary>
        /// Time delay between sending keep alive packets to the other Robot.
        /// </summary>
        private TimeSpan _keepalivePeriod = TimeSpan.FromSeconds(1);
        public TimeSpan KeepalivePeriod
        {
            get => _keepalivePeriod;
            set
            {
                ThrowIfLocked();
                _keepalivePeriod = value;
            }
        }

        /// <summary>
        /// The QoS level that MQTT is to use.
        /// NOTE: The default value should work in most cases.
        ///       Look at the MQTT documentation before editing this value.
        /// </summary>
        private MqttQualityOfServiceLevel _qosLevel = MqttQualityOfServiceLevel.ExactlyOnce;
        public MqttQualityOfServiceLevel QOSLevel
        {
            get => _qosLevel;
            set
            {
               ThrowIfLocked();
               _qosLevel = value;
            }
        }
        
        /// <summary>
        /// Options for using TLS with MQTT.
        /// </summary>
        private MqttClientTlsOptions _tlsOptions = new MqttClientTlsOptions()
        {
            UseTls = false,
            IgnoreCertificateRevocationErrors = true,
            IgnoreCertificateChainErrors = true,
            AllowUntrustedCertificates = true
        };
        public MqttClientTlsOptions TLSOptions
        {
            get => _tlsOptions;
            set
            {
                ThrowIfLocked();
                _tlsOptions = value;
            }
        }

        /// <summary>
        /// Whether the broker should clean the session info when a lient connects
        /// </summary>
        private bool _cleanSession = true;
        public bool CleanSession
        {
            get => _cleanSession;
            set
            {
                ThrowIfLocked();
                _cleanSession = value;
            }
        }

        protected override void ValidateInternal()
        {
            if (string.IsNullOrEmpty(RobotName))
            {
                throw new ConfigurationException($"{nameof(RobotName)} must be set");
            }
            
            if (!UseAutoDiscovery && BrokerPort < 1)
            {
                throw new ConfigurationException($"{nameof(BrokerPort)} must be set");
            }

            if (!IsBroker && !UseAutoDiscovery && string.IsNullOrEmpty(BrokerIPAddress))
            {
                throw new ConfigurationException($"{nameof(BrokerIPAddress)} must be set");
            }

            if (TLSOptions == null)
            {
                throw new ConfigurationException($"{nameof(TLSOptions)} must be set");
            }

            if (!string.IsNullOrEmpty(BrokerIPAddress))
            {
                var isValid = IPAddress.TryParse(BrokerIPAddress, out _);
                if (!isValid)
                {
                    throw new ConfigurationException($"Invalid value for {nameof(BrokerIPAddress)} provided");
                }
            }

            if (AutoDiscoveryBroadcastPort < 1)
            {
                throw new ConfigurationException($"Invalid value for {nameof(AutoDiscoveryBroadcastPort)} provided");
            }

            Lock();
        }
    }
}