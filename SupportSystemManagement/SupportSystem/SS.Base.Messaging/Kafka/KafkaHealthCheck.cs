using Confluent.Kafka;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SS.Base.Messaging.Kafka;

internal sealed class KafkaHealthCheck : IHealthCheck
{
    private readonly KafkaTopicManager _topics;

    public KafkaHealthCheck(KafkaTopicManager topics) => _topics = topics;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        try
        {
            var metadata = await Task.Run(() => _topics.Admin.GetMetadata(TimeSpan.FromSeconds(3)), cancellationToken);
            return metadata.Brokers.Count > 0
                ? HealthCheckResult.Healthy($"{metadata.Brokers.Count} Kafka broker(s) reachable")
                : new HealthCheckResult(context.Registration.FailureStatus, "No Kafka brokers in metadata");
        }
        catch (KafkaException ex)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, "Kafka unreachable", ex);
        }
    }
}
