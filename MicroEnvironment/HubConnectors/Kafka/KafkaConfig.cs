using System;
using System.Collections.Generic;
using System.Text;

namespace MicroEnvironment.HubConnectors.Kafka
{
    public class KafkaConfig
    {
        //
        // Summary:
        //     Client group id string. All clients sharing the same group.id belong to the same
        //     group. default: '' importance: high
        public string GroupId { get; set; }
        //
        // Summary:
        //     Client's public key string (PEM format) used for authentication. default: ''
        //     importance: low
        public string SslCertificatePem { get; set; }
        //
        // Summary:
        //     Path to client's public key (PEM) used for authentication. default: '' importance:
        //     low
        public string SslCertificateLocation { get; set; }
        //
        // Summary:
        //     Client's private key string (PEM format) used for authentication. default: ''
        //     importance: low
        public string SslKeyPem { get; set; }
        //
        // Summary:
        //     Private key passphrase (for use with `ssl.key.location` and `set_ssl_cert()`)
        //     default: '' importance: low
        public string SslKeyPassword { get; set; }
        //
        // Summary:
        //     Path to client's private key (PEM) used for authentication. default: '' importance:
        //     low
        public string SslKeyLocation { get; set; }
        //
        // Summary:
        //     The client uses the TLS ClientHello signature_algorithms extension to indicate
        //     to the server which signature/hash algorithm pairs may be used in digital signatures.
        //     See manual page for `SSL_CTX_set1_sigalgs_list(3)`. OpenSSL >= 1.0.2 required.
        //     default: '' importance: low
        public string SslSigalgsList { get; set; }
        //
        // Summary:
        //     The supported-curves extension in the TLS ClientHello message specifies the curves
        //     (standard/named, or 'explicit' GF(2^k) or GF(p)) the client is willing to have
        //     the server use. See manual page for `SSL_CTX_set1_curves_list(3)`. OpenSSL >=
        //     1.0.2 required. default: '' importance: low
        public string SslCurvesList { get; set; }
        //
        // Summary:
        //     A cipher suite is a named combination of authentication, encryption, MAC and
        //     key exchange algorithm used to negotiate the security settings for a network
        //     connection using TLS or SSL network protocol. See manual page for `ciphers(1)`
        //     and `SSL_CTX_set_cipher_list(3). default: '' importance: low
        public string SslCipherSuites { get; set; }
        //
        // Summary:
        //     Protocol used to communicate with brokers. default: plaintext importance: high
        public SecurityProtocol? SecurityProtocol { get; set; }
        //
        // Summary:
        //     Older broker versions (before 0.10.0) provide no way for a client to query for
        //     supported protocol features (ApiVersionRequest, see `api.version.request`) making
        //     it impossible for the client to know what features it may use. As a workaround
        //     a user may set this property to the expected broker version and the client will
        //     automatically adjust its feature set accordingly if the ApiVersionRequest fails
        //     (or is disabled). The fallback broker version will be used for `api.version.fallback.ms`.
        //     Valid values are: 0.9.0, 0.8.2, 0.8.1, 0.8.0. Any other value >= 0.10, such as
        //     0.10.2.1, enables ApiVersionRequests. default: 0.10.0 importance: medium
        public string BrokerVersionFallback { get; set; }
        //
        // Summary:
        //     Dictates how long the `broker.version.fallback` fallback is used in the case
        //     the ApiVersionRequest fails. **NOTE**: The ApiVersionRequest is only issued when
        //     a new connection to the broker is made (such as after an upgrade). default: 0
        //     importance: medium
        public int? ApiVersionFallbackMs { get; set; }
        //
        // Summary:
        //     Timeout for broker API version requests. default: 10000 importance: low
        public int? ApiVersionRequestTimeoutMs { get; set; }
        //
        // Summary:
        //     File or directory path to CA certificate(s) for verifying the broker's key. Defaults:
        //     On Windows the system's CA certificates are automatically looked up in the Windows
        //     Root certificate store. On Mac OSX this configuration defaults to `probe`. It
        //     is recommended to install openssl using Homebrew, to provide CA certificates.
        //     On Linux install the distribution's ca-certificates package. If OpenSSL is statically
        //     linked or `ssl.ca.location` is set to `probe` a list of standard paths will be
        //     probed and the first one found will be used as the default CA certificate location
        //     path. If OpenSSL is dynamically linked the OpenSSL library's default path will
        //     be used (see `OPENSSLDIR` in `openssl version -a`). default: '' importance: low
        public string SslCaLocation { get; set; }
        //
        // Summary:
        //     Comma-separated list of Windows Certificate stores to load CA certificates from.
        //     Certificates will be loaded in the same order as stores are specified. If no
        //     certificates can be loaded from any of the specified stores an error is logged
        //     and the OpenSSL library's default CA location is used instead. Store names are
        //     typically one or more of: MY, Root, Trust, CA. default: Root importance: low
        public string SslCaCertificateStores { get; set; }
        //
        // Summary:
        //     Path to CRL for verifying broker's certificate validity. default: '' importance:
        //     low
        public string SslCrlLocation { get; set; }
        //
        // Summary:
        //     Path to client's keystore (PKCS#12) used for authentication. default: '' importance:
        //     low
        public string SslKeystoreLocation { get; set; }
        //
        // Summary:
        //     Client's keystore (PKCS#12) password. default: '' importance: low
        public string SslKeystorePassword { get; set; }
        //
        // Summary:
        //     Enable OpenSSL's builtin broker (server) certificate verification. This verification
        //     can be extended by the application by implementing a certificate_verify_cb. default:
        //     true importance: low
        public bool? EnableSslCertificateVerification { get; set; }
        //
        // Summary:
        //     Endpoint identification algorithm to validate broker hostname using broker certificate.
        //     https - Server (broker) hostname verification as specified in RFC2818. none -
        //     No endpoint verification. OpenSSL >= 1.0.2 required. default: none importance:
        //     low
        public SslEndpointIdentificationAlgorithm? SslEndpointIdentificationAlgorithm { get; set; }
        //
        // Summary:
        //     Kerberos principal name that Kafka runs as, not including /hostname@REALM default:
        //     kafka importance: low
        public string SaslKerberosServiceName { get; set; }
        //
        // Summary:
        //     This client's Kerberos principal name. (Not supported on Windows, will use the
        //     logon user's principal). default: kafkaclient importance: low
        public string SaslKerberosPrincipal { get; set; }
        //
        // Summary:
        //     Shell command to refresh or acquire the client's Kerberos ticket. This command
        //     is executed on client creation and every sasl.kerberos.min.time.before.relogin
        //     (0=disable). %{config.prop.name} is replaced by corresponding config object value.
        //     default: kinit -R -t "%{sasl.kerberos.keytab}" -k %{sasl.kerberos.principal}
        //     || kinit -t "%{sasl.kerberos.keytab}" -k %{sasl.kerberos.principal} importance:
        //     low
        public string SaslKerberosKinitCmd { get; set; }
        //
        // Summary:
        //     Path to Kerberos keytab file. This configuration property is only used as a variable
        //     in `sasl.kerberos.kinit.cmd` as ` ... -t "%{sasl.kerberos.keytab}"`. default:
        //     '' importance: low
        public string SaslKerberosKeytab { get; set; }
        //
        // Summary:
        //     Minimum time in milliseconds between key refresh attempts. Disable automatic
        //     key refresh by setting this property to 0. default: 60000 importance: low
        public int? SaslKerberosMinTimeBeforeRelogin { get; set; }
        //
        // Summary:
        //     SASL username for use with the PLAIN and SASL-SCRAM-.. mechanisms default: ''
        //     importance: high
        public string SaslUsername { get; set; }
        //
        // Summary:
        //     SASL password for use with the PLAIN and SASL-SCRAM-.. mechanism default: ''
        //     importance: high
        public string SaslPassword { get; set; }
        //
        // Summary:
        //     SASL/OAUTHBEARER configuration. The format is implementation-dependent and must
        //     be parsed accordingly. The default unsecured token implementation (see https://tools.ietf.org/html/rfc7515#appendix-A.5)
        //     recognizes space-separated name=value pairs with valid names including principalClaimName,
        //     principal, scopeClaimName, scope, and lifeSeconds. The default value for principalClaimName
        //     is "sub", the default value for scopeClaimName is "scope", and the default value
        //     for lifeSeconds is 3600. The scope value is CSV format with the default value
        //     being no/empty scope. For example: `principalClaimName=azp principal=admin scopeClaimName=roles
        //     scope=role1,role2 lifeSeconds=600`. In addition, SASL extensions can be communicated
        //     to the broker via `extension_NAME=value`. For example: `principal=admin extension_traceId=123`
        //     default: '' importance: low
        public string SaslOauthbearerConfig { get; set; }
        //
        // Summary:
        //     Enable the builtin unsecure JWT OAUTHBEARER token handler if no oauthbearer_refresh_cb
        //     has been set. This builtin handler should only be used for development or testing,
        //     and not in production. default: false importance: low
        public bool? EnableSaslOauthbearerUnsecureJwt { get; set; }
        //
        // Summary:
        //     Request broker's supported API versions to adjust functionality to available
        //     protocol features. If set to false, or the ApiVersionRequest fails, the fallback
        //     version `broker.version.fallback` will be used. **NOTE**: Depends on broker version
        //     >=0.10.0. If the request is not supported by (an older) broker the `broker.version.fallback`
        //     fallback is used. default: true importance: high
        public bool? ApiVersionRequest { get; set; }
        //
        // Summary:
        //     Signal that librdkafka will use to quickly terminate on rd_kafka_destroy(). If
        //     this signal is not set then there will be a delay before rd_kafka_wait_destroyed()
        //     returns true as internal threads are timing out their system calls. If this signal
        //     is set however the delay will be minimal. The application should mask this signal
        //     as an internal signal handler is installed. default: 0 importance: low
        public int? InternalTerminationSignal { get; set; }
        //
        // Summary:
        //     If enabled librdkafka will initialize the PRNG with srand(current_time.milliseconds)
        //     on the first invocation of rd_kafka_new() (required only if rand_r() is not available
        //     on your platform). If disabled the application must call srand() prior to calling
        //     rd_kafka_new(). default: true importance: low
        public bool? EnableRandomSeed { get; set; }
        //
        // Summary:
        //     List of plugin libraries to load (; separated). The library search path is platform
        //     dependent (see dlopen(3) for Unix and LoadLibrary() for Windows). If no filename
        //     extension is specified the platform-specific extension (such as .dll or .so)
        //     will be appended automatically. default: '' importance: low
        public string PluginLibraryPaths { get; set; }
        //
        // Summary:
        //     SASL mechanism to use for authentication. Supported: GSSAPI, PLAIN, SCRAM-SHA-256,
        //     SCRAM-SHA-512. **NOTE**: Despite the name, you may not configure more than one
        //     mechanism.
        public SaslMechanism? SaslMechanism { get; set; }
        //
        // Summary:
        //     This field indicates the number of acknowledgements the leader broker must receive
        //     from ISR brokers before responding to the request: Zero=Broker does not send
        //     any response/ack to client, One=The leader will write the record to its local
        //     log but will respond without awaiting full acknowledgement from all followers.
        //     All=Broker will block until message is committed by all in sync replicas (ISRs).
        //     If there are less than min.insync.replicas (broker configuration) in the ISR
        //     set the produce request will fail.
        public Acks? Acks { get; set; }
        //
        // Summary:
        //     Client identifier. default: rdkafka importance: low
        public string ClientId { get; set; }
        //
        // Summary:
        //     Initial list of brokers as a CSV list of broker host or host:port. The application
        //     may also use `rd_kafka_brokers_add()` to add brokers during runtime. default:
        //     '' importance: high
        public string BootstrapServers { get; set; }
        //
        // Summary:
        //     Maximum Kafka protocol request message size. Due to differing framing overhead
        //     between protocol versions the producer is unable to reliably enforce a strict
        //     max message limit at produce time and may exceed the maximum size by one message
        //     in protocol ProduceRequests, the broker will enforce the the topic's `max.message.bytes`
        //     limit (see Apache Kafka documentation). default: 1000000 importance: medium
        public int? MessageMaxBytes { get; set; }
        //
        // Summary:
        //     Maximum size for message to be copied to buffer. Messages larger than this will
        //     be passed by reference (zero-copy) at the expense of larger iovecs. default:
        //     65535 importance: low
        public int? MessageCopyMaxBytes { get; set; }
        //
        // Summary:
        //     Maximum Kafka protocol response message size. This serves as a safety precaution
        //     to avoid memory exhaustion in case of protocol hickups. This value must be at
        //     least `fetch.max.bytes` + 512 to allow for protocol overhead; the value is adjusted
        //     automatically unless the configuration property is explicitly set. default: 100000000
        //     importance: medium
        public int? ReceiveMessageMaxBytes { get; set; }
        //
        // Summary:
        //     Maximum number of in-flight requests per broker connection. This is a generic
        //     property applied to all broker communication, however it is primarily relevant
        //     to produce requests. In particular, note that other mechanisms limit the number
        //     of outstanding consumer fetch request per broker to one. default: 1000000 importance:
        //     low
        public int? MaxInFlight { get; set; }
        //
        // Summary:
        //     Non-topic request timeout in milliseconds. This is for metadata requests, etc.
        //     default: 60000 importance: low
        public int? MetadataRequestTimeoutMs { get; set; }
        //
        // Summary:
        //     Period of time in milliseconds at which topic and broker metadata is refreshed
        //     in order to proactively discover any new brokers, topics, partitions or partition
        //     leader changes. Use -1 to disable the intervalled refresh (not recommended).
        //     If there are no locally referenced topics (no topic objects created, no messages
        //     produced, no subscription or no assignment) then only the broker list will be
        //     refreshed every interval but no more often than every 10s. default: 300000 importance:
        //     low
        public int? TopicMetadataRefreshIntervalMs { get; set; }
        //
        // Summary:
        //     Metadata cache max age. Defaults to topic.metadata.refresh.interval.ms * 3 default:
        //     900000 importance: low
        public int? MetadataMaxAgeMs { get; set; }
        //
        // Summary:
        //     When a topic loses its leader a new metadata request will be enqueued with this
        //     initial interval, exponentially increasing until the topic metadata has been
        //     refreshed. This is used to recover quickly from transitioning leader brokers.
        //     default: 250 importance: low
        public int? TopicMetadataRefreshFastIntervalMs { get; set; }
        //
        // Summary:
        //     Sparse metadata requests (consumes less network bandwidth) default: true importance:
        //     low
        public bool? TopicMetadataRefreshSparse { get; set; }
        //
        // Summary:
        //     Apache Kafka topic creation is asynchronous and it takes some time for a new
        //     topic to propagate throughout the cluster to all brokers. If a client requests
        //     topic metadata after manual topic creation but before the topic has been fully
        //     propagated to the broker the client is requesting metadata from, the topic will
        //     seem to be non-existent and the client will mark the topic as such, failing queued
        //     produced messages with `ERR__UNKNOWN_TOPIC`. This setting delays marking a topic
        //     as non-existent until the configured propagation max time has passed. The maximum
        //     propagation time is calculated from the time the topic is first referenced in
        //     the client, e.g., on produce(). default: 30000 importance: low
        public int? TopicMetadataPropagationMaxMs { get; set; }
        //
        // Summary:
        //     Topic blacklist, a comma-separated list of regular expressions for matching topic
        //     names that should be ignored in broker metadata information as if the topics
        //     did not exist. default: '' importance: low
        public string TopicBlacklist { get; set; }
        //
        // Summary:
        //     A comma-separated list of debug contexts to enable. Detailed Producer debugging:
        //     broker,topic,msg. Consumer: consumer,cgrp,topic,fetch default: '' importance:
        //     medium
        public string Debug { get; set; }
        //
        // Summary:
        //     Default timeout for network requests. Producer: ProduceRequests will use the
        //     lesser value of `socket.timeout.ms` and remaining `message.timeout.ms` for the
        //     first message in the batch. Consumer: FetchRequests will use `fetch.wait.max.ms`
        //     + `socket.timeout.ms`. Admin: Admin requests will use `socket.timeout.ms` or
        //     explicitly set `rd_kafka_AdminOptions_set_operation_timeout()` value. default:
        //     60000 importance: low
        public int? SocketTimeoutMs { get; set; }
        //
        // Summary:
        //     Broker socket send buffer size. System default is used if 0. default: 0 importance:
        //     low
        public int? SocketSendBufferBytes { get; set; }
        //
        // Summary:
        //     Broker socket receive buffer size. System default is used if 0. default: 0 importance:
        //     low
        public int? SocketReceiveBufferBytes { get; set; }
        //
        // Summary:
        //     Enable TCP keep-alives (SO_KEEPALIVE) on broker sockets default: false importance:
        //     low
        public bool? SocketKeepaliveEnable { get; set; }
        //
        // Summary:
        //     Disable the Nagle algorithm (TCP_NODELAY) on broker sockets. default: false importance:
        //     low
        public bool? SocketNagleDisable { get; set; }
        //
        // Summary:
        //     Disconnect from broker when this number of send failures (e.g., timed out requests)
        //     is reached. Disable with 0. WARNING: It is highly recommended to leave this setting
        //     at its default value of 1 to avoid the client and broker to become desynchronized
        //     in case of request timeouts. NOTE: The connection is automatically re-established.
        //     default: 1 importance: low
        public int? SocketMaxFails { get; set; }
        //
        // Summary:
        //     How long to cache the broker address resolving results (milliseconds). default:
        //     1000 importance: low
        public int? BrokerAddressTtl { get; set; }
        //
        // Summary:
        //     Allowed broker IP address families: any, v4, v6 default: any importance: low
        public BrokerAddressFamily? BrokerAddressFamily { get; set; }
        //
        // Summary:
        //     The initial time to wait before reconnecting to a broker after the connection
        //     has been closed. The time is increased exponentially until `reconnect.backoff.max.ms`
        //     is reached. -25% to +50% jitter is applied to each reconnect backoff. A value
        //     of 0 disables the backoff and reconnects immediately. default: 100 importance:
        //     medium
        public int? ReconnectBackoffMs { get; set; }
        //
        // Summary:
        //     The maximum time to wait before reconnecting to a broker after the connection
        //     has been closed. default: 10000 importance: medium
        public int? ReconnectBackoffMaxMs { get; set; }
        //
        // Summary:
        //     librdkafka statistics emit interval. The application also needs to register a
        //     stats callback using `rd_kafka_conf_set_stats_cb()`. The granularity is 1000ms.
        //     A value of 0 disables statistics. default: 0 importance: high
        public int? StatisticsIntervalMs { get; set; }
        //
        // Summary:
        //     Disable spontaneous log_cb from internal librdkafka threads, instead enqueue
        //     log messages on queue set with `rd_kafka_set_log_queue()` and serve log callbacks
        //     or events through the standard poll APIs. **NOTE**: Log messages will linger
        //     in a temporary queue until the log queue has been set. default: false importance:
        //     low
        public bool? LogQueue { get; set; }
        //
        // Summary:
        //     Print internal thread name in log messages (useful for debugging librdkafka internals)
        //     default: true importance: low
        public bool? LogThreadName { get; set; }
        //
        // Summary:
        //     Log broker disconnects. It might be useful to turn this off when interacting
        //     with 0.9 brokers with an aggressive `connection.max.idle.ms` value. default:
        //     true importance: low
        public bool? LogConnectionClose { get; set; }
        //
        // Summary:
        //     A rack identifier for this client. This can be any string value which indicates
        //     where this client is physically located. It corresponds with the broker config
        //     `broker.rack`. default: '' importance: low
        public string ClientRack { get; set; }
    }

    //
    // Summary:
    //     SecurityProtocol enum values
    public enum SecurityProtocol
    {
        //
        // Summary:
        //     Plaintext
        Plaintext = 0,
        //
        // Summary:
        //     Ssl
        Ssl = 1,
        //
        // Summary:
        //     SaslPlaintext
        SaslPlaintext = 2,
        //
        // Summary:
        //     SaslSsl
        SaslSsl = 3
    }

    //
    // Summary:
    //     SslEndpointIdentificationAlgorithm enum values
    public enum SslEndpointIdentificationAlgorithm
    {
        //
        // Summary:
        //     None
        None = 0,
        //
        // Summary:
        //     Https
        Https = 1
    }

    //
    // Summary:
    //     SaslMechanism enum values
    public enum SaslMechanism
    {
        //
        // Summary:
        //     GSSAPI
        Gssapi = 0,
        //
        // Summary:
        //     PLAIN
        Plain = 1,
        //
        // Summary:
        //     SCRAM-SHA-256
        ScramSha256 = 2,
        //
        // Summary:
        //     SCRAM-SHA-512
        ScramSha512 = 3,
        //
        // Summary:
        //     OAUTHBEARER
        OAuthBearer = 4
    }

    //
    // Summary:
    //     Acks enum values
    public enum Acks
    {
        //
        // Summary:
        //     All
        All = -1,
        //
        // Summary:
        //     None
        None = 0,
        //
        // Summary:
        //     Leader
        Leader = 1
    }

    //
    // Summary:
    //     BrokerAddressFamily enum values
    public enum BrokerAddressFamily
    {
        //
        // Summary:
        //     Any
        Any = 0,
        //
        // Summary:
        //     V4
        V4 = 1,
        //
        // Summary:
        //     V6
        V6 = 2
    }
}
