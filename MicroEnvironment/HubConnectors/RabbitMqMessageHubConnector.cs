using Binaron.Serializer;
using Newtonsoft.Json;
using Newtonsoft.Json.Bson;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MicroEnvironment.HubConnectors
{
    public class RabbitMqMessageHubConnector<TMessage> : IMessageHubConnector<TMessage>, IDisposable
    {
        private readonly ConnectionFactory factory;

        public JsonSerializer Serializer { get; }

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

            this.Serializer = JsonSerializer.CreateDefault();

            conn = factory.CreateConnection();
            channelConsumer = conn.CreateModel();
            channelPublisher = conn.CreateModel();
            consumer = new AsyncEventingBasicConsumer(channelConsumer);
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

            consumer.Received += async (ch, ea) =>
            {
                var body = ea.Body.ToArray();
                using var stream = new MemoryStream(body);
                using BsonDataReader reader = new BsonDataReader(stream);

                await Task.Factory.StartNew(() => {
                    OnMessageHandle?.Invoke(
                        ea.Exchange + ea.RoutingKey,
                        Serializer.Deserialize<MicroEnvironmentMessage<TMessage>>(reader)
                        );
                });

                await Task.CompletedTask;
            };

            // this consumer tag identifies the subscription
            // when it has to be cancel
            string tag = channelConsumer.BasicConsume(messageName, true, consumer);
            channelConsumer.ModelShutdown += Channel_ModelShutdown;

            return Task.FromResult(messageName);
        }

        private void Channel_ModelShutdown(object sender, ShutdownEventArgs e)
        {

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
