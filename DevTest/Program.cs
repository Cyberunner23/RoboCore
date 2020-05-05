using System;
using System.Threading;

using Serilog;

using RoboCore.Config;

namespace DevTest
{
    static class Program
    {
        static void Main(string[] args)
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Async(x => x.Console()).CreateLogger();
         
            var config = new RoboCoreConfig();
            var transportConfig = config.DataTransport.GetConfig<MQTTConfig>();
            transportConfig.IsBroker = true;
            transportConfig.RobotName = "BROKER";
            
            var config2 = new RoboCoreConfig();
            var transportConfig2 = config.DataTransport.GetConfig<MQTTConfig>(); 
            transportConfig2.RobotName = "CLIENT";
            
            var roboCore = new RoboCore.RoboCore(config);
            var roboCore2 = new RoboCore.RoboCore(config2);

            roboCore.Start();
            roboCore2.Start();
            
            var publisher = roboCore.CreatePublisher<TestMessage>("test");
            var subscriber = roboCore2.CreateSubscriber<TestMessage>("test");
            subscriber.MessageReceived += OnMessageReceived;

            var currentChar = '1';
            ConsoleKeyInfo cki;
            do
            {
                if (Console.KeyAvailable)
                {
                    cki = Console.ReadKey(true);
                    currentChar = cki.KeyChar;
                    switch (cki.KeyChar)
                    {
                        case 'p':
                            publisher.PublishMessage(new TestMessage {Num = 5});
                            Log.Information("Sent message");
                            break;
                    }
                }
                Thread.Sleep(1);
            } while (currentChar != 'q');
            
            
            
            roboCore.Stop();
            roboCore2.Stop();
        }

        private static void OnMessageReceived(TestMessage message)
        {
            Log.Information($"Message received: {message.Num}");
        }


        private static void Example()
        {
            Log.Logger = new LoggerConfiguration().WriteTo.Async(x => x.Console()).CreateLogger();
            
            var core = new RoboCore.RoboCore();
            core.Start();

            core.CreatePublisher<TestMessage>("yay");
            var sub = core.CreateSubscriber<TestMessage>("controller");
            sub.MessageReceived += message => { /*Do something with message*/ };
        }
    }
}