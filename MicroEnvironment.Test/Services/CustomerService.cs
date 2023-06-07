using MicroEnvironment.Attributes;
using MicroEnvironment.HubConnectors.Kafka;
using MicroEnvironment.HubConnectors.RabbitMq;
using MicroEnvironment.Messages;
using Newtonsoft.Json;
using System;
using System.Threading;
using System.Threading.Tasks;
using Xunit.Abstractions;

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
        private readonly ITestOutputHelper output;

        public CustomerService(ITestOutputHelper output)
        {
            this.output = output;
        }

        [MicroServiceMethod]
        public Task<string> CustomerCreate(string message)
        {
            throw new System.Exception("sunucu hata fırlattı");
            //Thread.Sleep(1000);
            Interlocked.Increment(ref Counter);

            return Task.FromResult(message);
        }

        [MicroServiceMethod]
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

            Action<string, Exception, MicroEnvironmentMessage> OnExceptionEventHandler = (messageName, ex, message) =>
            {
                output.WriteLine("MessageName = " + messageName);
                output.WriteLine("Exception = " + ex.Message + " => " + ex.StackTrace);
                output.WriteLine("Message = " + JsonConvert.SerializeObject(message));
            };

            OnExceptionEventHandler += (messageName, ex, message) =>
            {
                output.WriteLine("MessageName = " + messageName);
                output.WriteLine("Exception = " + ex.Message + " => " + ex.StackTrace);
                output.WriteLine("Message = " + JsonConvert.SerializeObject(message));
            };

            OnExceptionEventHandler += (messageName, ex, message) =>
            {
                output.WriteLine("MessageName = " + messageName);
                output.WriteLine("Exception = " + ex.Message + " => " + ex.StackTrace);
                output.WriteLine("Message = " + JsonConvert.SerializeObject(message));
            };

            await new ServiceListener<ICustomerService>(config, this).ListenAsync(OnExceptionEventHandler);

            //MessageListener<string, string> CustomerCreateListener = new MessageListener<string, string>(
            //    nameof(CustomerService) + "_" + nameof(CustomerCreate),
            //    new RabbitMqMessageHubConnector<string>(config),
            //    new RabbitMqMessageHubConnector<string>(config));

            //CustomerCreateListener.OnExceptionHandlingEvent += (messageName, ex, message) =>
            //{
            //    output.WriteLine("MessageName = " + messageName);
            //    output.WriteLine("Exception = " + ex.Message + " => " + ex.StackTrace);
            //    output.WriteLine("Message = " + JsonConvert.SerializeObject(message));
            //};

            //MessageListener<string, string> CustomerDeleteListener = new MessageListener<string, string>(
            //    nameof(CustomerService) + "_" + nameof(CustomerDelete),
            //    new RabbitMqMessageHubConnector<string>(config),
            //    new RabbitMqMessageHubConnector<string>(config));

            //await CustomerCreateListener.StartAsync();
            //await CustomerDeleteListener.StartAsync();

            //CustomerCreateListener.Register(CustomerCreate);
            //CustomerDeleteListener.Register(CustomerDelete);
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
