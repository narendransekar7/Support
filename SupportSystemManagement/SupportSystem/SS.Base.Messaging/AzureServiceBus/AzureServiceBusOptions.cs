namespace SS.Base.Messaging.AzureServiceBus;

/// <summary>
/// `Messaging:AzureServiceBus` settings. Give either a connection string (local emulator, or a
/// namespace's shared access policy) or a namespace host name (Microsoft Entra ID auth through
/// DefaultAzureCredential - e.g. AKS workload identity, or `az login` locally).
/// Topics need the Standard or Premium tier; the Basic tier only has queues.
/// </summary>
public sealed class AzureServiceBusOptions
{
    /// <summary>
    /// "Endpoint=sb://&lt;ns&gt;.servicebus.windows.net/;SharedAccessKeyName=...;SharedAccessKey=..." or, for the
    /// emulator, "Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;".
    /// </summary>
    public string? ConnectionString { get; set; }

    /// <summary>"&lt;ns&gt;.servicebus.windows.net" - used with DefaultAzureCredential when no connection string is set.</summary>
    public string? FullyQualifiedNamespace { get; set; }

    /// <summary>Topic name prefix, the same as Kafka's: TicketCreated -> "support.ticket-created".</summary>
    public string TopicPrefix { get; set; } = "support";

    /// <summary>Prefix of every subscription this service creates. Defaults to the service name.</summary>
    public string? SubscriptionPrefix { get; set; }

    /// <summary>
    /// Create missing topics/subscriptions at startup / first publish. Needs "Manage" rights; turn it off
    /// for the emulator (it gets its entities from Config.json) or a send/listen-only policy.
    /// </summary>
    public bool AutoCreateEntities { get; set; } = true;

    /// <summary>Extra attempts after a handler throws; the subscription's MaxDeliveryCount is this + 1.</summary>
    public int MaxRetries { get; set; } = 3;

    /// <summary>Delay before a failed message is released for redelivery: delivery n waits n * this.</summary>
    public int RetryBackoffMs { get; set; } = 1000;

    /// <summary>Messages one subscription processes at a time. 1 keeps them (mostly) in order.</summary>
    public int MaxConcurrentCalls { get; set; } = 1;

    /// <summary>AMQP over WebSockets (port 443) instead of TCP 5671 - for networks that block 5671.</summary>
    public bool UseWebSockets { get; set; }
}
