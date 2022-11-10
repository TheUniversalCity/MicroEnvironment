using MicroEnvironment.HubConnectors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MicroEnvironment.Test
{
    class PaymentServiceRabbitMQClient //: ICustomerService
    {
        static readonly string QUEUE_NAME_OF_CUSTOMER_CREATE = nameof(ICustomerService).Substring(1) + "_" + nameof(CustomerCreate);
        static readonly string QUEUE_NAME_OF_CUSTOMER_DELETE = nameof(ICustomerService).Substring(1) + "_" + nameof(CustomerDelete);

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
