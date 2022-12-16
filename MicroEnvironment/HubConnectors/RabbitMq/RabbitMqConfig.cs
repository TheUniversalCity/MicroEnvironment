using System;
using System.Collections.Generic;
using System.Text;

namespace MicroEnvironment.HubConnectors.RabbitMq
{
    public class RabbitMqConfig
    {
        public string Host { get; set; } = "localhost";
        public int Port { get; set; } = 5672;
        public string Username { get; set; } = "guest";
        public string Password { get; set; } = "guest";
    }
}
