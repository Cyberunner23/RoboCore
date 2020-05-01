using RoboCore.Discovery;

namespace RoboCore.Config
{
    public class RoboCoreComponents
    {
        private RoboCoreConfig _config;

        public RoboCoreComponents(RoboCoreConfig config)
        {
            _config = config;
            Discovery = new AutoDiscovery(_config);
        }
        
        /// <summary>
        /// Method used by the Robots to figure out which one is the MQTT Broker and get its IP
        /// AutoDiscovery:
        ///     AutoDiscovery makes the RoboCore object that acts as the MQTT Broker broadcast its information over the LAN network.
        ///     The Robots wait and listen for these messages and connect to the MQTT Broker Robot 
        /// StaticDiscovery:
        ///     Static discovery makes the Robots connect to the  
        /// </summary>
        public IDiscovery Discovery { get; set; }
    }
}