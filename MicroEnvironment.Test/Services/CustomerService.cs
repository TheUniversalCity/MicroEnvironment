using MicroEnvironment.HubConnectors.Kafka;
using MicroEnvironment.HubConnectors.RabbitMq;
using MicroEnvironment.Messages;
using System.Threading;
using System.Threading.Tasks;

namespace MicroEnvironment.Test
{

    public interface ICustomerService
    {
        Task<string> CustomerCreate(string message);
        Task<string> CustomerDelete(string message);
    }

    public class CustomerService : ICustomerService
    {
        public int Counter;
        public Task<string> CustomerCreate(string message)
        {
            //Thread.Sleep(1000);
            Interlocked.Increment(ref Counter);
            
            return Task.FromResult(message);
        }

        public Task<string> CustomerDelete(string message)
        {
            Thread.Sleep(1000);
            return Task.FromResult(message);
        }

        public async Task ListenToRabbitMQ()
        {
            RabbitMqConfig config = new RabbitMqConfig
            {
                Host = "localhost"
            };

            MessageListener<string, string> CustomerCreateListener = new MessageListener<string, string>(
                nameof(CustomerService) + "_" + nameof(CustomerCreate),
                new RabbitMqMessageHubConnector<string>(config),
                new RabbitMqMessageHubConnector<string>(config));

            MessageListener<string, string> CustomerDeleteListener = new MessageListener<string, string>(
                nameof(CustomerService) + "_" + nameof(CustomerDelete),
                new RabbitMqMessageHubConnector<string>(config),
                new RabbitMqMessageHubConnector<string>(config));

            await CustomerCreateListener.StartAsync();
            await CustomerDeleteListener.StartAsync();

            CustomerCreateListener.Register(CustomerCreate);
            CustomerDeleteListener.Register(CustomerDelete);
        }

        public async Task ListenToKafka()
        {
            KafkaConfig config = new KafkaConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = "Grup1"
            };
            MessageListener<string, string> CustomerCreateListener = new MessageListener<string, string>(
                nameof(CustomerService) + "_" + nameof(CustomerCreate),
            new KafkaMessageHubConnector<string>(config),
            new KafkaMessageHubConnector<string>(config));

            MessageListener<string, string> CustomerDeleteListener = new MessageListener<string, string>(
                nameof(CustomerService) + "_" + nameof(CustomerDelete),
                new KafkaMessageHubConnector<string>(config),
                new KafkaMessageHubConnector<string>(config));

            await CustomerCreateListener.StartAsync();
            await CustomerDeleteListener.StartAsync();

            CustomerCreateListener.Register(CustomerCreate);
            CustomerDeleteListener.Register(CustomerDelete);
        }
    }
}
