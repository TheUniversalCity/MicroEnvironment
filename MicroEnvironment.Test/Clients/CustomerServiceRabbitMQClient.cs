using MicroEnvironment.HubConnectors.RabbitMq;
using MicroEnvironment.Messages;
using RabbitMQ.Client;
using System.Threading.Tasks;

namespace MicroEnvironment.Test
{
    class CustomerServiceRabbitMQClient : ICustomerService
    {
        static readonly string QUEUE_NAME_OF_CUSTOMER_CREATE = nameof(ICustomerService).Substring(1) + "_" + nameof(CustomerCreate);
        static readonly string QUEUE_NAME_OF_CUSTOMER_DELETE = nameof(ICustomerService).Substring(1) + "_" + nameof(CustomerDelete);

        static RabbitMqConfig config = new RabbitMqConfig
        {
            Host = "localhost"
        };
        static IConnection connection;

        static CustomerServiceRabbitMQClient()
        {
            var factory = new ConnectionFactory
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

            connection = factory.CreateConnection();
        }

        private MessageSender<string, string> CustomerCreateMessageHub { get; set; } = new MessageSender<string, string>(
            QUEUE_NAME_OF_CUSTOMER_CREATE,
            new RabbitMqMessageHubConnector<string>(config),
            new RabbitMqMessageHubConnector<string>(config));

        private MessageSender<string, string> CustomerDeleteMessageHub { get; set; } = new MessageSender<string, string>(
            QUEUE_NAME_OF_CUSTOMER_DELETE,
            new RabbitMqMessageHubConnector<string>(config),
            new RabbitMqMessageHubConnector<string>(config));

        public async Task StartAsync()
        {
            await CustomerCreateMessageHub.StartAsync();
            await CustomerDeleteMessageHub.StartAsync();
        }

        public Task<string> CustomerCreate(string message)
        {
            return CustomerCreateMessageHub.Send(message);
        }

        public Task<string> CustomerDelete(string message)
        {
            return CustomerDeleteMessageHub.Send(message);
        }
    }
}
