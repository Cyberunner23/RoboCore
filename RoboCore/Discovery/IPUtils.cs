using System;
using System.Linq;
using System.Net;
using System.Net.Sockets;

namespace RoboCore.Discovery
{
    public static class IPUtils
    {
        public static IPAddress GetLocalIP()
        {
            var selfHostName = Dns.GetHostName();
            var selfHostEntry = Dns.GetHostEntry(selfHostName);
            var selfAddress = selfHostEntry.AddressList.FirstOrDefault(x => x.AddressFamily == AddressFamily.InterNetwork);
            
            if (selfAddress == null)
            {
                throw new Exception("Could not acquire our own local IP Address");    
            }

            return selfAddress;
        }
    }
}