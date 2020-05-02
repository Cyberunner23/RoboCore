using RoboCore.Config;

namespace RoboCore.Discovery
{
    public class AutoDiscovery : IDiscovery
    {
        private RoboCoreConfig _config;

        public AutoDiscovery(RoboCoreConfig config)
        {
            _config = config;
        }

        public void Start(bool isBroker)
        {
            throw new System.NotImplementedException();
        }

        public void Stop()
        {
            throw new System.NotImplementedException();
        }
    }
}