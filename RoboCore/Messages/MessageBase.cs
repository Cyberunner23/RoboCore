using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;

namespace RoboCore.Messages
{
    public class MessageBase
    {
        public string GetMessageTypeHash()
        {
            var properties = this.GetType().GetProperties();
            var propertyStrings = properties.Select(x => $"{x.Name}:{x.PropertyType}:{x.CanWrite}");
            var propertiesString = String.Join('\n', propertyStrings);
            
            using (var hasher = SHA256.Create())
            {
                var hash = hasher.ComputeHash(Encoding.UTF8.GetBytes(propertiesString));
                var hashString = System.Convert.ToBase64String(hash);
                return hashString;
            }
        }
    }
}