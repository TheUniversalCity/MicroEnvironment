using MicroEnvironment.Messages;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MicroEnvironment.HubConnectors.RabbitMq
{
    public class RabbitMqMessageHubConnector<TMessage> : IMessageHubConnector<TMessage>, IDisposable
    {
        private readonly ConnectionFactory factory;
        private IConnection externalConnection;
        private IConnection conn;
        private IModel channelConsumer;
        private IModel channelPublisher;
        private AsyncEventingBasicConsumer consumer;
        private bool disposedValue;

        private JsonSerializer Serializer { get; }

        public event Func<string, MicroEnvironmentMessage<TMessage>, Task> OnMessageHandle;
        public event Action<string> OnConnectionDown;

        public RabbitMqMessageHubConnector(RabbitMqConfig config) : this()
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
        }

        public RabbitMqMessageHubConnector(IConnection conn) : this()
        {
            this.externalConnection = conn;
        }

        private RabbitMqMessageHubConnector()
        {
            this.Serializer = JsonSerializer.CreateDefault();
        }

        public Task Send(string messageName, MicroEnvironmentMessage<TMessage> message, CancellationToken cancellationToken = default)
        {
            using var ms = new MemoryStream();
            using BsonDataWriter writer = new BsonDataWriter(ms);

            Serializer.Serialize(writer, message);

            channelPublisher.BasicPublish("", messageName, null, ms.ToArray());

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

        private void _tempConnection_ConnectionShutdown(object sender, ShutdownEventArgs e)
        {
            OnConnectionDown?.Invoke(e.ReplyText);
        }

        public async Task StartAsync()
        {
            await Task.Yield();

            Dispose(false);

            var _tempConnection = default(IConnection);

            if (externalConnection != null)
            {
                _tempConnection = externalConnection;
            }
            else if (factory != null)
            {
                _tempConnection = conn = factory.CreateConnection();
            }
            else
            {
                throw new Exception("ConnectionFactory required!");
            }

            _tempConnection.ConnectionShutdown += _tempConnection_ConnectionShutdown;

            channelConsumer = _tempConnection.CreateModel();
            channelPublisher = _tempConnection.CreateModel();

            channelConsumer.ModelShutdown += Channel_ModelShutdown;
            channelPublisher.ModelShutdown += Channel_ModelShutdown;

            consumer = new AsyncEventingBasicConsumer(channelConsumer);

            consumer.Received += async (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                using var stream = new MemoryStream(body);
                using BsonDataReader reader = new BsonDataReader(stream);

                //channelConsumer.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);

                await OnMessageHandle?.Invoke(
                    ea.Exchange + ea.RoutingKey,
                    Serializer.Deserialize<MicroEnvironmentMessage<TMessage>>(reader));
            };
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposedValue)
            {
                if (disposing)
                {
                    // TODO: dispose managed state (managed objects)
                    channelConsumer?.Close();
                    channelPublisher?.Close();
                    conn?.Close();

                    channelConsumer?.Dispose();
                    channelPublisher?.Dispose();
                    conn?.Dispose();
                }

                // TODO: free unmanaged resources (unmanaged objects) and override finalizer
                // TODO: set large fields to null
                disposedValue = true;
            }
        }

        // TODO: override finalizer only if 'Dispose(bool disposing)' has code to free unmanaged resources
        ~RabbitMqMessageHubConnector()
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
