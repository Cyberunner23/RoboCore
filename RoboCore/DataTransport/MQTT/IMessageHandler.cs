namespace RoboCore.DataTransport.MQTT
{
    public interface IMessageHandler
    {
        public bool HandleMessageReceived(string serializedMessage);
    }
}