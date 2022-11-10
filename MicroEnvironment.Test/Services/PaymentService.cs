using MicroEnvironment.HubConnectors;
using System;
using System.Threading.Tasks;

namespace MicroEnvironment.Test
{

    public interface IPaymentService
    {
        Task PaymentExecuted(string message);
    }

    public class PaymentService : IPaymentService
    {
        public Task PaymentExecuted(string message)
        {
            throw new NotImplementedException();
        }

        public void ListenToRabbitMQ()
        {
            RabbitMqConfig config = new RabbitMqConfig
            {
                Host = "localhost"
            };

            MessageListener<string> PaymentExecutedListener = new MessageListener<string>(
                nameof(CustomerService) + "_" + nameof(PaymentExecuted),
                new RabbitMqMessageHubConnector<string>(config));

            PaymentExecutedListener.Register(PaymentExecuted);
        }
    }
}
