using System;

namespace RoboCore.Exceptions
{
    public class ConfigurationException : Exception
    {
        public ConfigurationException(string message) : base(message) { }
    }
}