using Binaron.Serializer;
using Confluent.Kafka;
using MicroEnvironment.Exceptions;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;

namespace MicroEnvironment.HubConnectors
{
    public class KafkaMessageHubConnector<TMessage> : IMessageHubConnector<TMessage>, IDisposable
    {
        private IProducer<byte[], byte[]> Producer { get; }
        private IConsumer<byte[], byte[]> Consumer { get; }

        public event Func<string, MicroEnvironmentMessage<TMessage>, Task> OnMessageHandle;

        public KafkaMessageHubConnector(string clientName)
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = "localhost:9092",
            };
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = "localhost:9092",
                GroupId = clientName,
                EnableAutoCommit = true,
                AllowAutoCreateTopics = true,
                AutoOffsetReset = AutoOffsetReset.Earliest,
            };

            Producer = new ProducerBuilder<byte[], byte[]>(producerConfig)
                //.SetKeySerializer(new AvroSerializer<byte[]>(SchemaRegistry))
                //.SetValueSerializer(new AvroSerializer<byte[]>(SchemaRegistry))
                .Build();

            Consumer = new ConsumerBuilder<byte[], byte[]>(consumerConfig)
                //.SetKeyDeserializer(new AvroDeserializer<byte[]>(SchemaRegistry).AsSyncOverAsync())
                //.SetValueDeserializer(new AvroDeserializer<byte[]>(SchemaRegistry).AsSyncOverAsync())
                .Build();
        }

        public async Task Send(string messageName, MicroEnvironmentMessage<TMessage> message, CancellationToken cancellationToken = default)
        {
            using var ms = new MemoryStream();
            BinaronConvert.Serialize(message, ms);

            var result = await Producer.ProduceAsync(messageName, new Message<byte[], byte[]>
            {
                Key = message.MessageId.ToByteArray(),
                Value = ms.ToArray(),
                Timestamp = new Timestamp(message.Timestamp)
            });

            if (result.Status == PersistenceStatus.NotPersisted)
            {
                throw new TranmissionException();
            }
        }

        public Task<string> Subscribe(string messageName)
        {
            if (string.IsNullOrEmpty(messageName))
            {
                messageName = "AutoGenerate_" + Guid.NewGuid().ToString().Replace("-", "");
            }

            Consumer.Subscribe(messageName);

            Thread.Sleep(100);

            var task = new Task(async (consumer) =>
            {
                var _consumer = consumer as IConsumer<byte[], byte[]>;

                while (true)
                {
                    ConsumeResult<byte[], byte[]> consumeResult = default;

                    try
                    {
                        consumeResult = _consumer.Consume();
                    }
                    catch (ConsumeException ex) when (ex.Error.Code == ErrorCode.UnknownTopicOrPart)
                    {
                        continue;
                    }

                    using var ms = new MemoryStream(consumeResult.Message.Value);
                    
                    await OnMessageHandle?.Invoke(consumeResult.Topic, BinaronConvert.Deserialize<MicroEnvironmentMessage<TMessage>>(ms));
                }
            }, Consumer);

            task.Start();

            task.ContinueWith(c =>
            {
                if (c.IsFaulted)
                {
                    throw c.Exception;
                }
            });

            return Task.FromResult(messageName);
        }

        protected virtual void Dispose(bool disposing)
        {
            Consumer.Dispose();
            Producer.Dispose();
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
    }
}
