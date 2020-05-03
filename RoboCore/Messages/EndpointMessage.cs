using System.Collections.Generic;

namespace RoboCore.Messages
{
    public class EndpointMessage : IWrappedMessage
    {
        public string Source { get; set; }
        public string Destination { get; set; }
        public string Payload { get; set; }
        public string DataIntegrityValues { get; set; }
    }
}