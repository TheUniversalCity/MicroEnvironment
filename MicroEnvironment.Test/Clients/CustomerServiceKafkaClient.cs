using MicroEnvironment.HubConnectors;
using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace MicroEnvironment.Test
{
    class CustomerServiceKafkaClient //: ICustomerService
    {
        static string QUEUE_NAME_OF_CUSTOMER_CREATE = nameof(ICustomerService).Substring(1) + "_" + nameof(CustomerCreate);
        static string QUEUE_NAME_OF_CUSTOMER_DELETE = nameof(ICustomerService).Substring(1) + "_" + nameof(CustomerDelete);

        private MessageSender<string, string> CustomerCreateMessageHub { get; set; } = new MessageSender<string, string>(
            QUEUE_NAME_OF_CUSTOMER_CREATE,
            new KafkaMessageHubConnector<string>("Grup1"),
            new KafkaMessageHubConnector<string>("Grup1"));

        private MessageSender<string, string> CustomerDeleteMessageHub { get; set; } = new MessageSender<string, string>(
            QUEUE_NAME_OF_CUSTOMER_DELETE,
            new KafkaMessageHubConnector<string>("Grup1"),
            new KafkaMessageHubConnector<string>("Grup1"));

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
