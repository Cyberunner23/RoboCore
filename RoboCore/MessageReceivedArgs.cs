namespace RoboCore
{
    public class MessageReceivedArgs
    {
        public string SerializedMessage { get; set; }
        public bool DeserializationPassed { get; set; }
    }
}