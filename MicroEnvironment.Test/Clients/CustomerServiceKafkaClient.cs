using MicroEnvironment.HubConnectors.Kafka;
using MicroEnvironment.Messages;
using System.Threading.Tasks;

namespace MicroEnvironment.Test
{
    class CustomerServiceKafkaClient //: ICustomerService
    {
        static string QUEUE_NAME_OF_CUSTOMER_CREATE = nameof(ICustomerService).Substring(1) + "_" + nameof(CustomerCreate);
        static string QUEUE_NAME_OF_CUSTOMER_DELETE = nameof(ICustomerService).Substring(1) + "_" + nameof(CustomerDelete);

        static KafkaConfig config = new KafkaConfig
        {
            BootstrapServers = "localhost",
            GroupId = "Grup1"
        };

        private MessageSender<string, string> CustomerCreateMessageHub { get; set; } = new MessageSender<string, string>(
            QUEUE_NAME_OF_CUSTOMER_CREATE,
            new KafkaMessageHubConnector<string>(config),
            new KafkaMessageHubConnector<string>(config));

        private MessageSender<string, string> CustomerDeleteMessageHub { get; set; } = new MessageSender<string, string>(
            QUEUE_NAME_OF_CUSTOMER_DELETE,
            new KafkaMessageHubConnector<string>(config),
            new KafkaMessageHubConnector<string>(config));

        public Task<string> CustomerCreate(string message)
        {
            return CustomerCreateMessageHub.Send(message);
        }

        public Task<string> CustomerDelete(string message)
        {
            return CustomerDeleteMessageHub.Send(message);
        }

        public async Task StartAsync()
        {
            await CustomerCreateMessageHub.StartAsync();
            await CustomerDeleteMessageHub.StartAsync();
        }
    }
}
