using System;
using System.Collections.Generic;

namespace MicroEnvironment.Messages
{
    public sealed class MicroEnvironmentMessage<T>
    {
        public MicroEnvironmentMessage()
        {
                
        }

        public MicroEnvironmentMessage(T message)
        {
            this.Message = message;
        }

        public Guid MessageId { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public DateTime Timestamp { get; set; }
        public T Message { get; set; }
        public string Error { get; set; }
    }
}
