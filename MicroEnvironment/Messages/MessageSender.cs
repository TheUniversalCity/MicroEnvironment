using MicroEnvironment.Exceptions;
using MicroEnvironment.HubConnectors;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace MicroEnvironment.Messages
{
    public class MessageSender<TRequest> : IDisposable
    {
        private bool disposedValue;

        public string MessageName { get; }
        private IMessageHubConnector<TRequest> HubRequestConnector { get; }

        public MessageSender(string messageName, IMessageHubConnector<TRequest> hubRequestConnector)
        {
            MessageName = messageName;
            HubRequestConnector = hubRequestConnector;
        }

        public Task Send(TRequest message)
        {
            return HubRequestConnector.Send(MessageName, new MicroEnvironmentMessage<TRequest>(message) { MessageId = Guid.NewGuid(), Timestamp = DateTime.Now });
        }

        public async Task StartAsync()
        {
            await HubRequestConnector.StartAsync();
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    HubRequestConnector.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~MessageSender()
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

    public class MessageSender<TRequest, TResponse> : IDisposable
    {
        public ConcurrentDictionary<Guid, TaskCompletionSource<TResponse>> dictionary = new ConcurrentDictionary<Guid, TaskCompletionSource<TResponse>>();
        private bool disposedValue;

        public string MessageName { get; }
        public string ReplyTo { get; set; }

        private IMessageHubConnector<TRequest> HubRequestConnector { get; }
        private IMessageHubConnector<TResponse> HubResponseConnector { get; }
        private readonly int Timeout;
        public MessageSender(
            string messageName, 
            IMessageHubConnector<TRequest> hubRequestConnector, 
            IMessageHubConnector<TResponse> hubResponseConnector,
            int timeout = 20000)
        {
            MessageName = messageName;
            HubRequestConnector = hubRequestConnector;
            HubResponseConnector = hubResponseConnector;
            Timeout = timeout;
        }

        private Task HubResponseConnector_OnMessageHandle(string queueName, MicroEnvironmentMessage<TResponse> response)
        {
            if(dictionary.TryRemove(response.MessageId, out var tcs))
            {
                if (string.IsNullOrEmpty(response.Error))
                {
                    tcs.SetResult(response.Message);
                }
                else
                {
                    tcs.SetException(new ServiceException(response.Error));
                }
            }

            return Task.CompletedTask;
        }

        public async Task StartAsync()
        {
            await HubRequestConnector.StartAsync();
            await HubResponseConnector.StartAsync();

            HubResponseConnector.OnMessageHandle += HubResponseConnector_OnMessageHandle;

            ReplyTo = await this.HubResponseConnector.Subscribe(null);

            this.HubResponseConnector.OnConnectionDown += HubResponseConnector_OnConnectionDown;
        }

        private void HubResponseConnector_OnConnectionDown(string reason)
        {
            ReplyTo = null;
        }

        public async Task<TResponse> Send(TRequest message)
        {
            var tcs = new TaskCompletionSource<TResponse>(TaskCreationOptions.RunContinuationsAsynchronously);

            using var ct = new CancellationTokenSource(Timeout);

            ct.Token.Register(() => tcs.TrySetCanceled(), useSynchronizationContext: false);

            var microMessage = new MicroEnvironmentMessage<TRequest>(message)
            {
                MessageId = Guid.NewGuid(),
                Timestamp = DateTime.Now,
                Headers = { 
                    { MicroEnvironmentMessageHeaders.REPLY_TO, ReplyTo ?? await this.HubResponseConnector.Subscribe(null)}, 
                    { MicroEnvironmentMessageHeaders.CANCELLATION_TIMEOUT, Timeout.ToString() }
                }
            };

            dictionary.TryAdd(microMessage.MessageId, tcs);

            await HubRequestConnector.Send(MessageName, microMessage);

            return await tcs.Task;
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    HubRequestConnector.Dispose();
                    HubResponseConnector.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~MessageSender()
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
