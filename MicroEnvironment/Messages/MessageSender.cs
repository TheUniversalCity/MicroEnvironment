using MicroEnvironment.Exceptions;
using MicroEnvironment.HubConnectors;
using System;
using System.Collections.Concurrent;
using System.Threading;
using System.Threading.Tasks;

namespace MicroEnvironment.Messages
{
    public class MessageSender<TRequest>
    {
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
    }

    public class MessageSender<TRequest, TResponse>
    {
        public ConcurrentDictionary<Guid, TaskCompletionSource<TResponse>> dictionary = new ConcurrentDictionary<Guid, TaskCompletionSource<TResponse>>();

        public string MessageName { get; }
        public string ReplTo { get; set; }

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

            ReplTo = await this.HubResponseConnector.Subscribe(null);
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
                Headers = { { MicroEnvironmentMessageHeaders.REPLY_TO, ReplTo } }
            };

            dictionary.TryAdd(microMessage.MessageId, tcs);

            await HubRequestConnector.Send(MessageName, microMessage);

            return await tcs.Task;
        }
    }
}
