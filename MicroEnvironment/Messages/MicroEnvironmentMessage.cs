using System;
using System.Collections.Generic;

namespace MicroEnvironment.Messages
{
    public class MicroEnvironmentMessage
    {
        public MicroEnvironmentMessage()
        {

        }

        public MicroEnvironmentMessage(object message)
        {
            this.Message = message;
        }

        public Guid MessageId { get; set; }
        public Dictionary<string, string> Headers { get; set; } = new Dictionary<string, string>();
        public DateTime Timestamp { get; set; }
        public object Message { get; set; }
        public string Error { get; set; }
    }

    public sealed class MicroEnvironmentMessage<T> : MicroEnvironmentMessage
    {
        public MicroEnvironmentMessage()
        {
                
        }

        public MicroEnvironmentMessage(T message)
        {
            this.Message = message;
        }

        public new T Message { get { return (T)base.Message; } set { base.Message = value; } }
    }
}
