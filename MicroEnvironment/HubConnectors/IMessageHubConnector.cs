using System;
using System.Threading;
using System.Threading.Tasks;

namespace MicroEnvironment.HubConnectors
{
    public interface IMessageHubConnector<TMessage>
    {
        event Func<string, MicroEnvironmentMessage<TMessage>, Task> OnMessageHandle;
        Task Send(string messageName, MicroEnvironmentMessage<TMessage> microEnvironmentMessage, CancellationToken cancellationToken = default);
        Task<string> Subscribe(string messageName);
    }
}
