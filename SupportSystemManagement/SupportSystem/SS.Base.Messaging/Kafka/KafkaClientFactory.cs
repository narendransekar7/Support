using Confluent.Kafka;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SS.Base.Messaging.Kafka;

/// <summary>
/// Builds the Confluent clients with the shared connection/security settings.
/// </summary>
internal sealed class KafkaClientFactory
{
    private readonly KafkaOptions _options;
    private readonly string _clientId;

    public KafkaClientFactory(IOptions<KafkaOptions> options, MessagingServiceName serviceName)
    {
        _options = options.Value;
        _clientId = serviceName.Value;
    }

    public KafkaOptions Options => _options;

    public IProducer<string, byte[]> CreateProducer(ILogger logger)
    {
        var config = Apply(new ProducerConfig
        {
            // acks=all + idempotence: the broker confirms only after every in-sync replica has the
            // message, and producer retries can't write it twice or out of order.
            Acks = Acks.All,
            EnableIdempotence = true,
            // Wait up to 5 ms to batch messages going to the same partition.
            LingerMs = 5,
        });

        return new ProducerBuilder<string, byte[]>(config)
            .SetLogHandler((_, m) => LogClientMessage(logger, "producer", m))
            .SetErrorHandler((_, e) => LogClientError(logger, "producer", e))
            .Build();
    }

    public IConsumer<string, byte[]> CreateConsumer(string groupId, ILogger logger)
    {
        var config = Apply(new ConsumerConfig
        {
            GroupId = groupId,
            // A brand-new group starts at the oldest retained message instead of only new ones.
            AutoOffsetReset = AutoOffsetReset.Earliest,
            // Offsets are committed by hand only after the handler succeeded (at-least-once).
            EnableAutoCommit = false,
        });

        return new ConsumerBuilder<string, byte[]>(config)
            .SetLogHandler((_, m) => LogClientMessage(logger, "consumer", m))
            .SetErrorHandler((_, e) => LogClientError(logger, "consumer", e))
            // Rebalances are worth seeing while learning: start a second replica and watch partitions move.
            .SetPartitionsAssignedHandler((_, partitions) => logger.LogInformation(
                "Consumer group {GroupId} assigned partitions {Partitions}", groupId,
                string.Join(", ", partitions.Select(p => $"{p.Topic}[{p.Partition.Value}]"))))
            .SetPartitionsRevokedHandler((_, partitions) => logger.LogInformation(
                "Consumer group {GroupId} revoked partitions {Partitions}", groupId,
                string.Join(", ", partitions.Select(p => $"{p.Topic}[{p.Partition.Value}]"))))
            .Build();
    }

    public IAdminClient CreateAdminClient(ILogger logger) => new AdminClientBuilder(Apply(new AdminClientConfig()))
        .SetLogHandler((_, m) => LogClientMessage(logger, "admin", m))
        .SetErrorHandler((_, e) => LogClientError(logger, "admin", e))
        .Build();

    private T Apply<T>(T config) where T : ClientConfig
    {
        config.BootstrapServers = _options.BootstrapServers;
        config.ClientId = _clientId;

        if (!string.IsNullOrWhiteSpace(_options.SecurityProtocol))
            config.SecurityProtocol = Enum.Parse<SecurityProtocol>(_options.SecurityProtocol, ignoreCase: true);
        if (!string.IsNullOrWhiteSpace(_options.SaslMechanism))
            config.SaslMechanism = Enum.Parse<SaslMechanism>(_options.SaslMechanism, ignoreCase: true);
        if (!string.IsNullOrWhiteSpace(_options.SaslUsername))
            config.SaslUsername = _options.SaslUsername;
        if (!string.IsNullOrWhiteSpace(_options.SaslPassword))
            config.SaslPassword = _options.SaslPassword;
        if (!string.IsNullOrWhiteSpace(_options.SslCaPem))
            config.SslCaPem = _options.SslCaPem;

        return config;
    }

    // librdkafka's own log lines repeat what the error callback reports, so they go to Debug.
    private static void LogClientMessage(ILogger logger, string client, LogMessage message) =>
        logger.LogDebug("librdkafka {Client} [{Level}] {Facility}: {Message}", client, message.Level, message.Facility, message.Message);

    // While the broker is down librdkafka reports every reconnect attempt (Local_Transport) every few
    // seconds - Debug; "all brokers down" and anything else is worth a warning.
    private static void LogClientError(ILogger logger, string client, Error error)
    {
        var level = error.IsFatal ? LogLevel.Error : error.Code == ErrorCode.Local_Transport ? LogLevel.Debug : LogLevel.Warning;
        logger.Log(level, "Kafka {Client} error {Code}: {Reason}", client, error.Code, error.Reason);
    }
}
