
using System;
using System.Transactions;
using RoboCore.Config;
using RoboCore.DataTransport;
using RoboCore.DataTransport.MQTT;
using Serilog;

namespace RoboCore
{
    /// <summary>
    /// Main object for using RoboCore, sets up publishers and subscribers and runs the whole operation
    /// </summary>
    public class RoboCore : IRoboCore
    {
        private static readonly RoboCoreConfig DefaultConfig = new RoboCoreConfig();

        private readonly RoboCoreConfig _config;

        private bool _isRunning = false;
        private readonly IDataTransport _dataTransport;

        public RoboCore() : this(DefaultConfig) { }

        public RoboCore(RoboCoreConfig config)
        {
            _config = config;
            _config.Validate();
            _dataTransport = _config.DataTransport;
        }

        public bool Start()
        {
            Log.Information("Starting RoboCore");
            
            try
            {
                _dataTransport.Start();
            }
            catch (Exception e)
            {
                Log.Error($"{e}");
                return false;
            }

            Log.Information("RoboCore Started");
            _isRunning = true;
            return true;
        }

        public void Stop()
        {
            Log.Information("Stopping RoboCore");
            
            if (!_isRunning)
            {
                return;
            }
            
            _dataTransport.Stop();
            _isRunning = false;
            Log.Information("RoboCore Stopped");
        }

        public IPublisher<TMessage> CreatePublisher<TMessage>(string topic)
        {
            ThrowIfStopped();
            return _dataTransport.CreatePublisher<TMessage>(topic);
        }

        public ISubscriber<TMessage> CreateSubscriber<TMessage>(string topic)
        {
            ThrowIfStopped();
            return _dataTransport.CreateSubscriber<TMessage>(topic);
        }

        public IServiceEndpoint<TRequest, TResponse> CreateServiceEndpoint<TRequest, TResponse>(string topic)
        {
            ThrowIfStopped();
            return _dataTransport.CreateServiceEndpoint<TRequest, TResponse>(topic);
        }

        public IClientEndpoint<TRequest, TResponse> CreateClientEndpoint<TRequest, TResponse>(string topic)
        {
            ThrowIfStopped();
            return _dataTransport.CreateClientEndpoint<TRequest, TResponse>(topic);
        }

        private void ThrowIfStopped()
        {
            if (!_isRunning)
            {
                throw new InvalidOperationException("RoboCore must be started before performing this action");
            }
        }
    }
}