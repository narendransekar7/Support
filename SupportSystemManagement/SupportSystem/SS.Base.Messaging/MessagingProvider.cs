namespace SS.Base.Messaging;

/// <summary>
/// The broker a deployment uses, chosen with `Messaging:Provider` (env var `Messaging__Provider`).
/// Every provider stays in the code; the setting only picks which one is wired up at startup.
/// </summary>
public enum MessagingProvider
{
    /// <summary>MassTransit over RabbitMQ (the original transport, incl. the MassTransit saga state machine).</summary>
    RabbitMq,

    /// <summary>Apache Kafka through Confluent.Kafka (topics + consumer groups, provider-neutral orchestrator).</summary>
    Kafka,

    /// <summary>Azure Service Bus - reserved for a later phase; selecting it fails at startup.</summary>
    AzureServiceBus
}
