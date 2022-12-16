﻿using Confluent.Kafka;
using MicroEnvironment.Exceptions;
using MicroEnvironment.Messages;
using System;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace MicroEnvironment.HubConnectors.Kafka
{
    public class KafkaMessageHubConnector<TMessage> : IMessageHubConnector<TMessage>, IDisposable
    {
        private IProducer<byte[], byte[]> Producer { get; }
        private IConsumer<byte[], byte[]> Consumer { get; }

        public event Func<string, MicroEnvironmentMessage<TMessage>, Task> OnMessageHandle;

        public KafkaMessageHubConnector(KafkaConfig config)
        {
            var producerConfig = new ProducerConfig
            {
                BootstrapServers = config.BootstrapServers, // "localhost:9092",
                Acks = (Confluent.Kafka.Acks)config.Acks,
                ApiVersionFallbackMs = config.ApiVersionFallbackMs,
                ApiVersionRequest = config.ApiVersionRequest,
                ApiVersionRequestTimeoutMs = config.ApiVersionRequestTimeoutMs,
                BrokerAddressFamily = (Confluent.Kafka.BrokerAddressFamily)config.BrokerAddressFamily,
                BrokerAddressTtl = config.BrokerAddressTtl,
                BrokerVersionFallback = config.BrokerVersionFallback,
                ClientId = config.ClientId,
                ClientRack = config.ClientRack,
                Debug = config.Debug,
                EnableRandomSeed = config.EnableRandomSeed,
                EnableSaslOauthbearerUnsecureJwt = config.EnableSaslOauthbearerUnsecureJwt,
                EnableSslCertificateVerification = config.EnableSslCertificateVerification,
                InternalTerminationSignal = config.InternalTerminationSignal,
                LogConnectionClose = config.LogConnectionClose,
                LogQueue = config.LogQueue,
                LogThreadName = config.LogThreadName,
                MaxInFlight = config.MaxInFlight,
                MessageCopyMaxBytes = config.MessageCopyMaxBytes,
                MessageMaxBytes = config.MessageMaxBytes,
                MetadataMaxAgeMs = config.MetadataMaxAgeMs,
                MetadataRequestTimeoutMs = config.MetadataRequestTimeoutMs,
                PluginLibraryPaths = config.PluginLibraryPaths,
                ReceiveMessageMaxBytes = config.ReceiveMessageMaxBytes,
                ReconnectBackoffMaxMs = config.ReconnectBackoffMaxMs,
                ReconnectBackoffMs = config.ReconnectBackoffMs,
                SaslKerberosKeytab = config.SaslKerberosKeytab,
                SaslKerberosKinitCmd = config.SaslKerberosKinitCmd,
                SaslKerberosMinTimeBeforeRelogin = config.SaslKerberosMinTimeBeforeRelogin,
                SaslKerberosPrincipal = config.SaslKerberosPrincipal,
                SaslKerberosServiceName = config.SaslKerberosServiceName,
                SaslMechanism = (Confluent.Kafka.SaslMechanism)config.SaslMechanism,
                SaslOauthbearerConfig = config.SaslOauthbearerConfig,
                SaslPassword = config.SaslPassword,
                SaslUsername = config.SaslUsername,
                SecurityProtocol = (Confluent.Kafka.SecurityProtocol)config.SecurityProtocol,
                SocketKeepaliveEnable = config.SocketKeepaliveEnable,
                SocketMaxFails = config.SocketMaxFails,
                SocketNagleDisable = config.SocketNagleDisable,
                SocketReceiveBufferBytes = config.SocketReceiveBufferBytes,
                SocketSendBufferBytes = config.SocketSendBufferBytes,
                SocketTimeoutMs = config.SocketTimeoutMs,
                SslCaCertificateStores = config.SslCaLocation,
                SslCaLocation = config.SslCaLocation,
                SslCertificateLocation = config.SslCertificateLocation,
                SslCertificatePem = config.SslCertificatePem,
                SslCipherSuites = config.SslCipherSuites,
                SslCrlLocation = config.SslCrlLocation,
                SslCurvesList = config.SslCurvesList,
                SslEndpointIdentificationAlgorithm = (Confluent.Kafka.SslEndpointIdentificationAlgorithm)config.SslEndpointIdentificationAlgorithm,
                SslKeyLocation = config.SslKeyLocation,
                SslKeyPassword = config.SslKeyPassword,
                SslKeyPem = config.SslKeyPem,
                SslKeystoreLocation = config.SslKeystoreLocation,
                SslKeystorePassword = config.SslKeystorePassword,
                SslSigalgsList = config.SslSigalgsList,
                StatisticsIntervalMs = config.StatisticsIntervalMs,
                TopicBlacklist = config.TopicBlacklist,
                TopicMetadataPropagationMaxMs = config.TopicMetadataPropagationMaxMs,
                TopicMetadataRefreshFastIntervalMs = config.TopicMetadataRefreshFastIntervalMs,
                TopicMetadataRefreshIntervalMs = config.TopicMetadataRefreshIntervalMs,
                TopicMetadataRefreshSparse = config.TopicMetadataRefreshSparse
            };
            var consumerConfig = new ConsumerConfig
            {
                BootstrapServers = config.BootstrapServers, // "localhost:9092",
                GroupId = config.GroupId,
                EnableAutoCommit = true,
                AllowAutoCreateTopics = true,
                AutoOffsetReset = AutoOffsetReset.Earliest,
                Acks = (Confluent.Kafka.Acks)config.Acks,
                ApiVersionFallbackMs = config.ApiVersionFallbackMs,
                ApiVersionRequest = config.ApiVersionRequest,
                ApiVersionRequestTimeoutMs = config.ApiVersionRequestTimeoutMs,
                BrokerAddressFamily = (Confluent.Kafka.BrokerAddressFamily)config.BrokerAddressFamily,
                BrokerAddressTtl = config.BrokerAddressTtl,
                BrokerVersionFallback = config.BrokerVersionFallback,
                ClientId = config.ClientId,
                ClientRack = config.ClientRack,
                Debug = config.Debug,
                EnableRandomSeed = config.EnableRandomSeed,
                EnableSaslOauthbearerUnsecureJwt = config.EnableSaslOauthbearerUnsecureJwt,
                EnableSslCertificateVerification = config.EnableSslCertificateVerification,
                InternalTerminationSignal = config.InternalTerminationSignal,
                LogConnectionClose = config.LogConnectionClose,
                LogQueue = config.LogQueue,
                LogThreadName = config.LogThreadName,
                MaxInFlight = config.MaxInFlight,
                MessageCopyMaxBytes = config.MessageCopyMaxBytes,
                MessageMaxBytes = config.MessageMaxBytes,
                MetadataMaxAgeMs = config.MetadataMaxAgeMs,
                MetadataRequestTimeoutMs = config.MetadataRequestTimeoutMs,
                PluginLibraryPaths = config.PluginLibraryPaths,
                ReceiveMessageMaxBytes = config.ReceiveMessageMaxBytes,
                ReconnectBackoffMaxMs = config.ReconnectBackoffMaxMs,
                ReconnectBackoffMs = config.ReconnectBackoffMs,
                SaslKerberosKeytab = config.SaslKerberosKeytab,
                SaslKerberosKinitCmd = config.SaslKerberosKinitCmd,
                SaslKerberosMinTimeBeforeRelogin = config.SaslKerberosMinTimeBeforeRelogin,
                SaslKerberosPrincipal = config.SaslKerberosPrincipal,
                SaslKerberosServiceName = config.SaslKerberosServiceName,
                SaslMechanism = (Confluent.Kafka.SaslMechanism)config.SaslMechanism,
                SaslOauthbearerConfig = config.SaslOauthbearerConfig,
                SaslPassword = config.SaslPassword,
                SaslUsername = config.SaslUsername,
                SecurityProtocol = (Confluent.Kafka.SecurityProtocol)config.SecurityProtocol,
                SocketKeepaliveEnable = config.SocketKeepaliveEnable,
                SocketMaxFails = config.SocketMaxFails,
                SocketNagleDisable = config.SocketNagleDisable,
                SocketReceiveBufferBytes = config.SocketReceiveBufferBytes,
                SocketSendBufferBytes = config.SocketSendBufferBytes,
                SocketTimeoutMs = config.SocketTimeoutMs,
                SslCaCertificateStores = config.SslCaLocation,
                SslCaLocation = config.SslCaLocation,
                SslCertificateLocation = config.SslCertificateLocation,
                SslCertificatePem = config.SslCertificatePem,
                SslCipherSuites = config.SslCipherSuites,
                SslCrlLocation = config.SslCrlLocation,
                SslCurvesList = config.SslCurvesList,
                SslEndpointIdentificationAlgorithm = (Confluent.Kafka.SslEndpointIdentificationAlgorithm)config.SslEndpointIdentificationAlgorithm,
                SslKeyLocation = config.SslKeyLocation,
                SslKeyPassword = config.SslKeyPassword,
                SslKeyPem = config.SslKeyPem,
                SslKeystoreLocation = config.SslKeystoreLocation,
                SslKeystorePassword = config.SslKeystorePassword,
                SslSigalgsList = config.SslSigalgsList,
                StatisticsIntervalMs = config.StatisticsIntervalMs,
                TopicBlacklist = config.TopicBlacklist,
                TopicMetadataPropagationMaxMs = config.TopicMetadataPropagationMaxMs,
                TopicMetadataRefreshFastIntervalMs = config.TopicMetadataRefreshFastIntervalMs,
                TopicMetadataRefreshIntervalMs = config.TopicMetadataRefreshIntervalMs,
                TopicMetadataRefreshSparse = config.TopicMetadataRefreshSparse
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
            var result = await Producer.ProduceAsync(messageName, new Message<byte[], byte[]>
            {
                Key = message.MessageId.ToByteArray(),
                Value = JsonSerializer.SerializeToUtf8Bytes(message),
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

            var task = Task.Factory.StartNew(async (consumer) =>
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

                    await OnMessageHandle?.Invoke(
                        consumeResult.Topic,
                        JsonSerializer.Deserialize<MicroEnvironmentMessage<TMessage>>(new Memory<byte>(consumeResult.Message.Value).Span));
                }
            }, Consumer);

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