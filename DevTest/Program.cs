using System;

using Serilog;

using RoboCore.Config;

namespace DevTest
{
    class Program
    {
        static void Main(string[] args)
        {

            Log.Logger = new LoggerConfiguration().WriteTo.Console().CreateLogger();
         
            var config = new RoboCoreConfig();
            config.DataTransport.GetConfig<MQTTConfig>().IsBroker = true;
            
            var roboCore = new RoboCore.RoboCore(config);
            var roboCore2 = new RoboCore.RoboCore();

            roboCore.Start();
            Console.ReadKey();
            roboCore2.Start();
            Console.ReadKey();
            roboCore.Stop();
            roboCore2.Stop();
        }
    }
}