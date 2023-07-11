using MicroEnvironment.Messages;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MicroEnvironment.HubConnectors
{
    public interface IMessageHubConnector<TMessage> : IDisposable
    {
        event Func<string, MicroEnvironmentMessage<TMessage>, Task> OnMessageHandle;
        event Action<string> OnConnectionDown;
        Task Send(string messageName, MicroEnvironmentMessage<TMessage> microEnvironmentMessage, CancellationToken cancellationToken = default);
        Task<string> Subscribe(string messageName);
        Task StartAsync();
    }
}
