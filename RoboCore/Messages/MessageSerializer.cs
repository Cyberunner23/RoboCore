using System;
using System.IO;

using Newtonsoft.Json;
using Newtonsoft.Json.Bson;

namespace RoboCore.Messages
{
    public class PubSubMessageSerializer<TWrapper, TMessage> where TWrapper : IWrappedMessage, new()
    {
        public string Serialize(TMessage message)
        {
            var serializedMessage = BSONSerialize(message);
            var wrappedMessage = new TWrapper()
            {
                Payload = serializedMessage,
                // TODO(AFL): Message integrity values
            };

            var serialized = JsonConvert.SerializeObject(wrappedMessage);
            return serialized;
        }

        public TMessage Deserialize(string serialized)
        {
            var wrappedMessage = JsonConvert.DeserializeObject<TWrapper>(serialized);
            // TODO(AFL): Verify integrity values, error handling

            var message = BSONDeserialize(wrappedMessage.Payload);
            return message;
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