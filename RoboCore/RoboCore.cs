using System;

using Serilog;

using RoboCore.Config;
using RoboCore.DataTransport;

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

        public IPublisher<TMessage> CreatePublisher<TMessage>(string topic) where TMessage : class, new()
        {
            ThrowIfStopped();
            return _dataTransport.CreatePublisher<TMessage>(topic);
        }

        public ISubscriber CreateSubscriber<TMessage>(string topic, Action<TMessage> messageReceivedHandler) where TMessage : class, new()
        {
            ThrowIfStopped();

            if (messageReceivedHandler == null)
            {
                throw new ArgumentNullException(nameof(messageReceivedHandler));
            }
            
            return _dataTransport.CreateSubscriber(topic, messageReceivedHandler);
        }

        public IServiceEndpoint CreateServiceEndpoint<TRequest, TResponse>(string topic, Func<TRequest, TResponse> requestReceivedHandler)
            where TRequest : class, new() where TResponse : class, new()
        {
            ThrowIfStopped();
            
            if (requestReceivedHandler == null)
            {
                throw new ArgumentNullException(nameof(requestReceivedHandler));
            }

            return _dataTransport.CreateServiceEndpoint(topic, requestReceivedHandler);
        }

        public IClientEndpoint<TRequest, TResponse> CreateClientEndpoint<TRequest, TResponse>(string topic, Action<TResponse> responseReceivedHandler)
            where TRequest : class, new() where TResponse : class, new()
        {
            ThrowIfStopped();
            return _dataTransport.CreateClientEndpoint<TRequest, TResponse>(topic, responseReceivedHandler);
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