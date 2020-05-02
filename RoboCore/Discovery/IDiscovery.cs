using System.Net;

namespace RoboCore.Discovery
{
    public delegate void HostDiscoveredHandler(string networkID, IPAddress address, int port);
    
    public interface IDiscovery
    {
        public event HostDiscoveredHandler HostDiscovered;

        public void PublishPresence();
        
        public void Start();
        public void Stop();
    }
}