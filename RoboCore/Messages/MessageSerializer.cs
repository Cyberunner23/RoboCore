using System;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace RoboCore.Messages
{
    public class MessageSerializer<TWrapper, TMessage> where TWrapper : IWrappedMessage, new() where TMessage : class, new()
    {
        public string Serialize(TMessage message)
        {
            var wrapper = SerializeMessage(message);
            return SerializeWrapper(wrapper);
        }

        public TWrapper SerializeMessage(TMessage message)
        {
            var serializedMessage = BSONSerialize(message);
            var wrappedMessage = new TWrapper()
            {
                Payload = serializedMessage,
                // TODO(AFL): Message integrity values
            };

            return wrappedMessage;
        }

        public string SerializeWrapper(TWrapper wrapper)
        {
            return JsonConvert.SerializeObject(wrapper);
        }

        public bool Deserialize(string serialized, out TMessage message)
        {
            TWrapper wrappedMessage;
            if (!Deserialize(serialized, out wrappedMessage))
            {
                message = null;
                return false;
            }

            return Deserialize(wrappedMessage, out message);
        }

        public bool Deserialize(string serialized, out TWrapper wrappedMessage)
        {
            wrappedMessage = JsonConvert.DeserializeObject<TWrapper>(serialized);
            
            // TODO(AFL): Verify Integrity values, error handling
            return true;
        }

        public bool Deserialize(TWrapper wrappedMessage, out TMessage message)
        {
            message = BSONDeserialize(wrappedMessage.Payload);

            return true;
        }

        private string BSONSerialize(object obj)
        {
            var memStream = new MemoryStream();
            using (var writer = new BsonWriter(memStream))
            {
                var serializer = new JsonSerializer();
                serializer.Serialize(writer, obj);
            }

            return Convert.ToBase64String(memStream.ToArray());
        }

        private TMessage BSONDeserialize(string serialized)
        {
            var data = Convert.FromBase64String(serialized);
            var memStream = new MemoryStream(data);

            // TODO(AFL): Error handling
            using (var reader = new BsonReader(memStream))
            {
                var serializer = new JsonSerializer();
                var obj = serializer.Deserialize<TMessage>(reader);
                return obj;
            }
        }
    }
}