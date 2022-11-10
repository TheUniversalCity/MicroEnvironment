using MicroEnvironment.HubConnectors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MicroEnvironment.Test
{
    public class OrderServiceRabbitMQClient : IOrderService
    {
        static readonly string QUEUE_NAME_OF_CUSTOMER_CREATE = nameof(ICustomerService).Substring(1) + "_" + nameof(OrderCreate);
        static readonly string QUEUE_NAME_OF_CUSTOMER_DELETE = nameof(ICustomerService).Substring(1) + "_" + nameof(OderDelivered);

        static RabbitMqConfig config = new RabbitMqConfig
        {
            Host = "localhost"
        };

        private MessageSender<string, string> CustomerCreateMessageHub { get; set; } = new MessageSender<string, string>(
            QUEUE_NAME_OF_CUSTOMER_CREATE,
            new RabbitMqMessageHubConnector<string>(config),
            new RabbitMqMessageHubConnector<string>(config));

        private MessageSender<string, string> CustomerDeleteMessageHub { get; set; } = new MessageSender<string, string>(
            QUEUE_NAME_OF_CUSTOMER_DELETE,
            new RabbitMqMessageHubConnector<string>(config),
            new RabbitMqMessageHubConnector<string>(config));

        public Task OderDelivered(string message)
        {
            throw new NotImplementedException();
        }

        public Task OrderCreate(string message)
        {
            throw new NotImplementedException();
        }
    }
}
