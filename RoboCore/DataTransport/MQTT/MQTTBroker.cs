using System;
using System.Net;
using MQTTnet;
using MQTTnet.Server;
using RoboCore.Discovery;
using Serilog;

namespace RoboCore.DataTransport.MQTT
{
    public class MQTTBroker
    {
        private IMqttServer _broker;

        public static int DefaultPort = new MqttServerOptions().DefaultEndpointOptions.Port;

        public int Port { get; }
        public IPAddress IPAddress => IPUtils.GetLocalIP();

        public MQTTBroker(int port)
        {
            Port = port;
        }

        public void Start()
        {
            if (_broker != null)
            {
                throw new InvalidOperationException("Broker is already running");
            }
            
            _broker = new MqttFactory().CreateMqttServer();
            _broker.UseClientConnectedHandler(new MqttServerClientConnectedHandlerDelegate((args => Log.Debug($"Robot {args.ClientId} connected"))));
            _broker.UseClientDisconnectedHandler(new MqttServerClientDisconnectedHandlerDelegate((args => Log.Debug($"Robot {args.ClientId} disconnected"))));
            
            var options = new MqttServerOptions();
            options.DefaultEndpointOptions.Port = Port;
            
            Log.Information($"MQTT Broker: Starting on port {IPAddress}:{Port}");
            _broker.StartAsync(options).Wait();
            Log.Information("MQTT Broker: Started");
        }

        public void Stop()
        {
            if (_broker == null)
            {
                return;
            }
            
            _broker.StopAsync().Wait();
            _broker = null;
            Log.Information("MQTT Broker: Stopped");
        }
    }
}