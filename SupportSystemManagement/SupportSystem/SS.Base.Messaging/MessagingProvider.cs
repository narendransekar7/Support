namespace SS.Base.Messaging;

/// <summary>
/// The broker a deployment uses, chosen with `Messaging:Provider` (env var `Messaging__Provider`).
/// Every provider stays in the code; the setting only picks which one is wired up at startup.
/// </summary>
public enum MessagingProvider
{
    /// <summary>MassTransit over RabbitMQ (the original transport, incl. the MassTransit saga state machine).</summary>
    RabbitMq,

    /// <summary>Apache Kafka through Confluent.Kafka (topics + consumer groups, provider-neutral orchestrator). The default.</summary>
    Kafka,

    /// <summary>Azure Service Bus through Azure.Messaging.ServiceBus (topics + subscriptions, provider-neutral orchestrator).</summary>
    AzureServiceBus
}
