using System.Collections.Concurrent;
using Confluent.Kafka;
using Confluent.Kafka.Admin;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace SS.Base.Messaging.Kafka;

/// <summary>
/// Creates topics explicitly (with our partition count) instead of relying on the broker's
/// auto-create, which is turned off in docker-compose. Each topic is checked once per process.
/// </summary>
internal sealed class KafkaTopicManager : IDisposable
{
    private readonly KafkaOptions _options;
    private readonly ILogger<KafkaTopicManager> _logger;
    private readonly ConcurrentDictionary<string, byte> _ensured = new();
    private readonly Lazy<IAdminClient> _admin;

    public KafkaTopicManager(KafkaClientFactory factory, IOptions<KafkaOptions> options, ILogger<KafkaTopicManager> logger)
    {
        _options = options.Value;
        _logger = logger;
        _admin = new Lazy<IAdminClient>(() => factory.CreateAdminClient(logger));
    }

    public IAdminClient Admin => _admin.Value;

    public async Task EnsureTopicAsync(string topic)
    {
        if (!_options.AutoCreateTopics || _ensured.ContainsKey(topic))
            return;

        try
        {
            await Admin.CreateTopicsAsync(new[]
            {
                new TopicSpecification
                {
                    Name = topic,
                    NumPartitions = _options.Partitions,
                    ReplicationFactor = _options.ReplicationFactor,
                }
            }, new CreateTopicsOptions { RequestTimeout = TimeSpan.FromSeconds(15) });
            _logger.LogInformation("Created Kafka topic {Topic} ({Partitions} partitions, replication factor {ReplicationFactor})",
                topic, _options.Partitions, _options.ReplicationFactor);
        }
        catch (CreateTopicsException ex) when (ex.Results.All(r => r.Error.Code is ErrorCode.NoError or ErrorCode.TopicAlreadyExists))
        {
            // Another service (or an earlier run) created it first - fine.
        }

        _ensured.TryAdd(topic, 0);
    }

    public void Dispose()
    {
        if (_admin.IsValueCreated)
            _admin.Value.Dispose();
    }
}
