using MicroEnvironment.HubConnectors;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace MicroEnvironment.Test
{

    public interface ICustomerService
    {
        string CustomerCreate(string message);
        Task<string> CustomerDelete(string message);
    }

    public class CustomerService : ICustomerService
    {
        public int Counter;
        public string CustomerCreate(string message)
        {
            //Thread.Sleep(1000);
            Interlocked.Increment(ref Counter);
            
            return message;// Task.FromResult(message);
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
            new KafkaMessageHubConnector<string>(grupId),
            new KafkaMessageHubConnector<string>(grupId));

            MessageListener<string, string> CustomerDeleteListener = new MessageListener<string, string>(
                nameof(CustomerService) + "_" + nameof(CustomerDelete),
                new KafkaMessageHubConnector<string>(grupId),
                new KafkaMessageHubConnector<string>(grupId));

            CustomerCreateListener.Register(CustomerCreate);
            CustomerDeleteListener.Register(CustomerDelete);
        }
    }
}
