using System;
using System.Net;
using Newtonsoft.Json;
using RoboCore.Config;
using RoboCore.DataTransport.MQTT;
using Serilog;

namespace DevTest
{
    class Program
    {
        static void Main(string[] args)
        {

            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
         
            var config = new RoboCoreConfig()
            {
                IsBroker = true
            };
            var dataTransportBootstrapper = new MQTTDataTransportBootstrapper(config);
            var transport = dataTransportBootstrapper.CreateMQTTDataTransport();
            
            transport.Start();
            Console.ReadKey();
            transport.Stop();
        }
    }
}