using MicroEnvironment.Messages;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MicroEnvironment.HubConnectors.RabbitMq
{
    public class RabbitMqMessageHubConnector<TMessage> : IMessageHubConnector<TMessage>, IDisposable
    {
        private readonly ConnectionFactory factory;

        //public JsonSerializer Serializer { get; }

        private readonly IConnection conn;
        private readonly IModel channelConsumer;
        private readonly IModel channelPublisher;
        private readonly AsyncEventingBasicConsumer consumer;

        public event Func<string, MicroEnvironmentMessage<TMessage>, Task> OnMessageHandle;

        public RabbitMqMessageHubConnector(RabbitMqConfig config)
        {
            factory = new ConnectionFactory
            {
                UserName = config.Username,
                Password = config.Password,
                VirtualHost = "/",
                HostName = config.Host,
                Port = config.Port,
                DispatchConsumersAsync = true,
                AutomaticRecoveryEnabled = true,
                TopologyRecoveryEnabled = true,
                //NetworkRecoveryInterval = TimeSpan.FromSeconds(5),
                //RequestedHeartbeat = TimeSpan.FromHours(1),
                //UseBackgroundThreadsForIO = true,
                RequestedChannelMax = 0
            };

            //this.Serializer = JsonSerializer.CreateDefault();

            conn = factory.CreateConnection();
            channelConsumer = conn.CreateModel();
            channelPublisher = conn.CreateModel();

            channelConsumer.ModelShutdown += Channel_ModelShutdown;
            channelPublisher.ModelShutdown += Channel_ModelShutdown;

            consumer = new AsyncEventingBasicConsumer(channelConsumer);

            consumer.Received += async (ch, ea) =>
            {
                await OnMessageHandle?.Invoke(
                    ea.Exchange + ea.RoutingKey,
                    JsonSerializer.Deserialize<MicroEnvironmentMessage<TMessage>>(ea.Body.Span)
                    );
            };
        }

        public Task Send(string messageName, MicroEnvironmentMessage<TMessage> message, CancellationToken cancellationToken = default)
        {
            channelPublisher.BasicPublish("", messageName, null, JsonSerializer.SerializeToUtf8Bytes(message));

            return Task.CompletedTask;
        }

        public Task<string> Subscribe(string messageName)
        {
            if (string.IsNullOrEmpty(messageName))
            {
                messageName = channelConsumer.QueueDeclare().QueueName;
            }
            else
            {
                messageName = channelConsumer.QueueDeclare(messageName, false, false, false, null).QueueName;
            }

            // this consumer tag identifies the subscription
            // when it has to be cancel
            string tag = channelConsumer.BasicConsume(messageName, true, consumer);

            return Task.FromResult(messageName);
        }

        private void Channel_ModelShutdown(object sender, ShutdownEventArgs e)
        {
            // 
        }

        protected virtual void Dispose(bool disposing)
        {
            channelConsumer.Close();
            conn.Close();
            channelConsumer.Dispose();
            conn.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
