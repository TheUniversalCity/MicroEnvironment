using MicroEnvironment.HubConnectors;
using System;
using System.Threading.Tasks;

namespace MicroEnvironment.Test
{

    public interface IOrderService
    {
        Task OrderCreate(string message);
        Task OderDelivered(string message);
    }

    public class OrderService : IOrderService
    {
        IOrderService pService;

        public Task OrderCreate(string message)
        {
            throw new NotImplementedException();
        }

        public Task OderDelivered(string message)
        {
            throw new NotImplementedException();
        }

        public void ListenToRabbitMQ()
        {
            RabbitMqConfig config = new RabbitMqConfig
            {
                Host = "localhost"
            };

            MessageListener<string> OrderCreateListener = new MessageListener<string>(
                nameof(CustomerService) + "_" + nameof(OrderCreate),
                new RabbitMqMessageHubConnector<string>(config));
            
            OrderCreateListener.Register(OrderCreate);
        }
    }
}
