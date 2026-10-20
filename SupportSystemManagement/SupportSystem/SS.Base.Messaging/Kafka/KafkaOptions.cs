namespace SS.Base.Messaging.Kafka;

/// <summary>
/// `Messaging:Kafka` settings. Only <see cref="BootstrapServers"/> is needed locally; the security
/// settings are for a managed cluster (Confluent Cloud, Azure Event Hubs' Kafka endpoint).
/// </summary>
public sealed class KafkaOptions
{
    /// <summary>Broker list, e.g. "localhost:9092" (docker-compose.kafka.yml) or "kafka:9092" (in the compose network).</summary>
    public string BootstrapServers { get; set; } = "localhost:9092";

    /// <summary>Prefix of every consumer group this service creates. Defaults to the service name.</summary>
    public string? ConsumerGroup { get; set; }

    /// <summary>Topic name prefix: TicketCreated -> "support.ticket-created".</summary>
    public string TopicPrefix { get; set; } = "support";

    /// <summary>Create missing topics at startup / first publish (the broker's own auto-create is off).</summary>
    public bool AutoCreateTopics { get; set; } = true;

    /// <summary>Partitions per new topic - the most consumers of one group that can work in parallel.</summary>
    public int Partitions { get; set; } = 3;

    /// <summary>Copies of each partition. 1 for a single local broker; 3 in a real cluster.</summary>
    public short ReplicationFactor { get; set; } = 1;

    /// <summary>Extra attempts after a handler throws, before the message goes to the dead-letter topic.</summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>Delay before retry n is n * this.</summary>
    public int RetryBackoffMs { get; set; } = 1000;

    /// <summary>Plaintext, Ssl, SaslPlaintext or SaslSsl. Empty = Plaintext.</summary>
    public string? SecurityProtocol { get; set; }

    /// <summary>Plain, ScramSha256, ScramSha512 ...</summary>
    public string? SaslMechanism { get; set; }

    public string? SaslUsername { get; set; }

    public string? SaslPassword { get; set; }

    /// <summary>
    /// PEM certificate(s) of the CA that signed the brokers' TLS certificates, when it isn't a publicly trusted one -
    /// e.g. Strimzi's cluster CA (the "&lt;cluster&gt;-cluster-ca-cert" Secret, key ca.crt). Empty = the system trust store.
    /// </summary>
    public string? SslCaPem { get; set; }
}
