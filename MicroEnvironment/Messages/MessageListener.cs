using MicroEnvironment.Attributes;
using MicroEnvironment.HubConnectors;
using System;
using System.Threading.Tasks;

namespace MicroEnvironment.Messages
{
    public class MessageListener<TRequest>
    {
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
                       delege((envMessage as MicroEnvironmentMessage<TRequest>).Message);
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
                        await delege((envMessage as MicroEnvironmentMessage<TRequest>).Message);
                    }, envMessage);
                }
            };

            this.HubRequestConnector.Subscribe(MessageName);
        }

        public Task StartAsync()
        {
            return HubRequestConnector.StartAsync();
        }
    }

    public class MessageListener<TRequest, TResponse>
    {
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
                    Task.Factory.StartNew(async (envMessage) =>
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
                        }

                        var microMessage = new MicroEnvironmentMessage<TResponse>(result)
                        {
                            MessageId = _envMessage.MessageId,
                            Timestamp = _envMessage.Timestamp,
                            Error = ex?.Message
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
                    Task.Factory.StartNew(async (envMessage) =>
                    {
                        var _envMessage = envMessage as MicroEnvironmentMessage<TRequest>;
                        var result = await delege(_envMessage.Message);
                        var microMessage = new MicroEnvironmentMessage<TResponse>(result) { MessageId = _envMessage.MessageId, Timestamp = _envMessage.Timestamp };

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
    }
}
