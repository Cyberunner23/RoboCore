namespace RoboCore.Messages
{
    public interface IWrappedMessage
    {
        public string Payload { get; set; }
        public string DataIntegrityValues { get; set; }
    }
}