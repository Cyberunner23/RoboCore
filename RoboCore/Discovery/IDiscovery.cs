namespace RoboCore.Discovery
{
    public interface IDiscovery
    {
        public delegate void HostAppearedHandler(string ip);
        public delegate void HostDisappearedHandler(string ip);
        
        public void Start(bool isBroker);
        public void Stop();
    }
}