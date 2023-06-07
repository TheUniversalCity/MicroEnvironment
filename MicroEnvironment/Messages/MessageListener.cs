using MicroEnvironment.Attributes;
using MicroEnvironment.HubConnectors;
using System;
using System.Threading.Tasks;

namespace MicroEnvironment.Messages
{
    public class MessageListener
    {
        protected Action<string, Exception, MicroEnvironmentMessage> OnExceptionHandlingEventHandler;

        public event Action<string, Exception, MicroEnvironmentMessage> OnExceptionHandlingEvent {
            add
            {
                OnExceptionHandlingEventHandler += value;
            }
            remove
            {
                OnExceptionHandlingEventHandler -= value;
            }
        }
    }

    public class MessageListener<TRequest> : MessageListener, IDisposable
    {
        private bool disposedValue;

        public string MessageName { get; }
        private IMessageHubConnector<TRequest> HubRequestConnector { get; }

        public MessageListener(string messageName, IMessageHubConnector<TRequest> hubRequestConnector)
        {
            MessageName = messageName;
            HubRequestConnector = hubRequestConnector;
        }

        public void Register(Action<TRequest> delege)
        {
            this.HubRequestConnector.OnMessageHandle += async (messageName, envMessage) =>
           {
               if (messageName == MessageName)
               {
                   await Task.Factory.StartNew((envMessage) =>
                   {
                       var _envMessage = (envMessage as MicroEnvironmentMessage<TRequest>);

                       try
                       {
                           delege(_envMessage.Message);
                       }
                       catch (Exception _ex)
                       {
                           OnExceptionHandlingEventHandler?.Invoke(messageName, _ex, _envMessage);
                       }
                   }, envMessage);
               }
           };

            this.HubRequestConnector.Subscribe(MessageName);
        }

        [ForAwaitable]
        public void Register(Func<TRequest, Task> delege)
        {
            this.HubRequestConnector.OnMessageHandle += async (messageName, envMessage) =>
            {
                if (messageName == MessageName)
                {
                    await Task.Factory.StartNew(async (envMessage) =>
                    {
                        var _envMessage = (envMessage as MicroEnvironmentMessage<TRequest>);

                        try
                        {
                            await delege(_envMessage.Message);
                        }
                        catch (Exception _ex)
                        {
                            OnExceptionHandlingEventHandler?.Invoke(messageName, _ex, _envMessage);
                        }
                    }, envMessage);
                }
            };

            this.HubRequestConnector.Subscribe(MessageName);
        }

        public Task StartAsync()
        {
            return HubRequestConnector.StartAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    HubRequestConnector.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~MessageListener()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }

    public class MessageListener<TRequest, TResponse> : MessageListener, IDisposable
    {
        private bool disposedValue;

        public string MessageName { get; }
        private IMessageHubConnector<TRequest> HubRequestConnector { get; }
        private IMessageHubConnector<TResponse> HubResponseConnector { get; }

        public MessageListener(string messageName, IMessageHubConnector<TRequest> hubRequestConnector, IMessageHubConnector<TResponse> hubResponseConnector)
        {
            MessageName = messageName;
            HubRequestConnector = hubRequestConnector;
            HubResponseConnector = hubResponseConnector;
        }

        public void Register(Func<TRequest, TResponse> delege)
        {
            this.HubRequestConnector.OnMessageHandle += (messageName, envMessage) =>
            {
                if (MessageName == messageName)
                {
                    return Task.Factory.StartNew(async (envMessage) =>
                    {
                        TResponse result = default;
                        Exception ex = null;

                        var _envMessage = envMessage as MicroEnvironmentMessage<TRequest>;

                        try
                        {
                            result = delege(_envMessage.Message);
                        }
                        catch (Exception _ex)
                        {
                            ex = _ex;
                            OnExceptionHandlingEventHandler?.Invoke(messageName, _ex, _envMessage);
                        }

                        var microMessage = new MicroEnvironmentMessage<TResponse>(result)
                        {
                            MessageId = _envMessage.MessageId,
                            Timestamp = _envMessage.Timestamp,
                            Error = $"Exception: {ex?.Message}"
                        };

                        await this.HubResponseConnector.Send(
                            _envMessage.Headers[MicroEnvironmentMessageHeaders.REPLY_TO],
                            microMessage);
                    }, envMessage);

                }

                return Task.CompletedTask;
            };

            this.HubRequestConnector.Subscribe(MessageName);
        }

        [ForAwaitable]
        public void Register(Func<TRequest, Task<TResponse>> delege)
        {
            this.HubRequestConnector.OnMessageHandle += (messageName, envMessage) =>
            {
                if (MessageName == messageName)
                {
                    return Task.Factory.StartNew(async (envMessage) =>
                    {
                        TResponse result = default;
                        Exception ex = null;

                        var _envMessage = envMessage as MicroEnvironmentMessage<TRequest>;

                        try
                        {
                            result = await delege(_envMessage.Message);
                        }
                        catch (Exception _ex)
                        {
                            ex = _ex;
                            OnExceptionHandlingEventHandler?.Invoke(messageName, _ex, _envMessage);
                        }

                        var microMessage = new MicroEnvironmentMessage<TResponse>(result)
                        {
                            MessageId = _envMessage.MessageId,
                            Timestamp = _envMessage.Timestamp,
                            Error = $"Exception: {ex?.Message}"
                        };

                        await this.HubResponseConnector.Send(
                            _envMessage.Headers[MicroEnvironmentMessageHeaders.REPLY_TO],
                            microMessage);
                    }, envMessage);
                }

                return Task.CompletedTask;
            };

            this.HubRequestConnector.Subscribe(MessageName);
        }

        public async Task StartAsync()
        {
            await HubRequestConnector.StartAsync();
            await HubResponseConnector.StartAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    HubRequestConnector.Dispose();
                    HubRequestConnector.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~MessageListener()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: false);
        }

        public void Dispose()
        {
            // Do not change this code. Put cleanup code in 'Dispose(bool disposing)' method
            Dispose(disposing: true);
            GC.SuppressFinalize(this);
        }
    }
}
