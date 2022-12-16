using MicroEnvironment.HubConnectors;
using MicroEnvironment.HubConnectors.Kafka;
using MicroEnvironment.HubConnectors.RabbitMq;
using MicroEnvironment.Messages;
using System;
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

        public void ListenToRabbitMQ()
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

            CustomerCreateListener.Register(CustomerCreate);
            CustomerDeleteListener.Register(CustomerDelete);
        }

        public void ListenToKafka()
        {
            var grupId = "Grup1";
            MessageListener<string, string> CustomerCreateListener = new MessageListener<string, string>(
                nameof(CustomerService) + "_" + nameof(CustomerCreate),
            new KafkaMessageHubConnector<string>(new KafkaConfig { BootstrapServers = "localhost:9092", GroupId = grupId }),
            new KafkaMessageHubConnector<string>(new KafkaConfig { BootstrapServers = "localhost:9092", GroupId = grupId }));

            MessageListener<string, string> CustomerDeleteListener = new MessageListener<string, string>(
                nameof(CustomerService) + "_" + nameof(CustomerDelete),
                new KafkaMessageHubConnector<string>(new KafkaConfig { BootstrapServers = "localhost:9092", GroupId = grupId }),
                new KafkaMessageHubConnector<string>(new KafkaConfig { BootstrapServers = "localhost:9092", GroupId = grupId }));

            CustomerCreateListener.Register(CustomerCreate);
            CustomerDeleteListener.Register(CustomerDelete);
        }
    }
}
