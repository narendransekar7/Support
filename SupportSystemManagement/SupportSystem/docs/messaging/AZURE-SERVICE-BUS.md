# Azure Service Bus in the Support System

The same ticket workflow as [KAFKA.md](KAFKA.md), on Azure Service Bus. Kafka stays the default;
select Service Bus with `Messaging:Provider = AzureServiceBus`.

```jsonc
"Messaging": {
  "Provider": "AzureServiceBus",
  "AzureServiceBus": {
    "ConnectionString": "",          // a secret - set via env/user-secrets
    "FullyQualifiedNamespace": "",   // "<ns>.servicebus.windows.net" -> Microsoft Entra ID (DefaultAzureCredential)
    "AutoCreateEntities": true       // false for the emulator / a send-listen-only policy
  }
}
```

The code is in [`SS.Base.Messaging/AzureServiceBus/`](../../SS.Base.Messaging/AzureServiceBus). The handlers, the saga
(`TicketCreationOrchestrator`) and the hosts' `Subscribe<TMessage, THandler>()` lists are the same ones
Kafka uses. Only the transport differs.

## Concepts, and where they live in the code

| Concept | What it means | In this project |
|---|---|---|
| **Namespace** | The Service Bus server you connect to. Topics need the **Standard** tier or higher; Basic has queues only. | `ConnectionString` or `FullyQualifiedNamespace`. |
| **Topic** | A named destination for published messages. | One per message type, with the same names as Kafka: `support.ticket-created`. Created by `AzureServiceBusEntityManager` when `AutoCreateEntities` is on. |
| **Subscription** | A topic's per-consumer inbox. Every subscription gets its own copy of every message, like a Kafka consumer group. | One per handler: `<service>.<handler>`, e.g. `ss-email-api.ticket-created-email-handler`. Names are limited to 50 characters, so generic arguments are dropped (`ss-audit-api.audit-event-handler`). |
| **Peek-lock** | A received message is locked, not removed, until the consumer settles it. | `AzureServiceBusProcessorWorker` uses `AutoCompleteMessages = false`. |
| **Complete / Abandon / DeadLetter** | Settling a message: remove it, release it for redelivery, or park it. | Success → Complete. Failure → wait `n × RetryBackoffMs`, then Abandon. Last delivery or an unreadable body → DeadLetter with a reason. |
| **DeliveryCount / MaxDeliveryCount** | The broker counts deliveries. Past the subscription's maximum, it dead-letters the message by itself. | `MaxDeliveryCount = MaxRetries + 1` (4). |
| **Dead-letter sub-queue** | Every subscription has a built-in `$DeadLetterQueue`. | `<topic>/Subscriptions/<subscription>/$DeadLetterQueue`. Kafka needed a hand-made `.dlt` topic for this. |
| **MessageId / Subject / CorrelationId** | Native message properties. | MessageId = our event id (the de-duplication key), Subject = message type, CorrelationId = ticket id. occurred-at, source and traceparent go in application properties. |
| **Lock renewal** | A lock expires after `LockDuration` (1 min) unless renewed. | The processor renews it for up to 5 minutes (`MaxAutoLockRenewalDuration`). |
| **Ordering** | Not guaranteed across concurrent receivers. Strict FIFO needs **sessions**. | `MaxConcurrentCalls = 1` per subscription keeps messages mostly in order. The saga tolerates reordering anyway. |

## Kafka vs Service Bus vs RabbitMQ

| | Kafka | Azure Service Bus | RabbitMQ (MassTransit) |
|---|---|---|---|
| Fan-out | topic → consumer group per handler | topic → subscription per handler | exchange → queue per consumer |
| Message after success | stays until retention ends (replayable) | removed from that subscription | removed from that queue |
| Acknowledging | commit offset | Complete (peek-lock) | ack |
| Retries | hand-written (back-off loop) | Abandon + DeliveryCount | MassTransit retry |
| Dead letters | hand-made `.dlt` topic | built-in `$DeadLetterQueue` | MassTransit `_error` queue |
| Ordering | per partition, by key | sessions (not used here) | per queue |
| Scaling a consumer | at most one per partition | competing consumers on one subscription, any number | competing consumers, any number |
| Saga | `TicketCreationOrchestrator` | `TicketCreationOrchestrator` | MassTransit state machine |
| Runs locally | apache/kafka container | Service Bus emulator + SQL Server | rabbitmq container |

## Run it

### Docker (full stack, with the emulator)

```powershell
$env:MESSAGING_PROVIDER = "AzureServiceBus"; docker compose --profile servicebus up -d --build
```

- `--profile servicebus` adds the emulator and the SQL Server it stores state in.
- The services use the emulator's connection string by default, with `AutoCreateEntities=false`.
- The topics and subscriptions come from [`servicebus-emulator/Config.json`](../../servicebus-emulator/Config.json).
- Emulator health: <http://localhost:5300/health>.

To use a **real namespace** instead of the emulator, set these before `docker compose up` (no `--profile` needed):

```powershell
$env:AZURE_SERVICE_BUS_CONNECTION_STRING = "Endpoint=sb://<ns>.servicebus.windows.net/;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=<key>"
$env:AZURE_SERVICE_BUS_AUTO_CREATE = "true"
```

### Visual Studio

1. Start the emulator: `docker compose -f docker-compose.servicebus.yml up -d`. It uses host port 5672, so stop the RabbitMQ container first.
2. For each service, set the provider and connection string (user-secrets or launch profile environment variables):
   - `Messaging__Provider=AzureServiceBus`
   - `Messaging__AzureServiceBus__ConnectionString=Endpoint=sb://localhost;SharedAccessKeyName=RootManageSharedAccessKey;SharedAccessKey=SAS_KEY_VALUE;UseDevelopmentEmulator=true;`
   - `Messaging__AzureServiceBus__AutoCreateEntities=false`
3. Run the services as described in [KAFKA.md](KAFKA.md).

### AKS

- Create a **Standard** namespace in Azure. The services create their topics and subscriptions if the policy has *Manage* rights.
- Select the provider: `MESSAGING_PROVIDER: "AzureServiceBus"` in the ConfigMap, or `config.messagingProvider` in Helm.
- Then authenticate in one of two ways:
  - **Connection string:** `AZURE_SERVICE_BUS_CONNECTION_STRING` in the Secret (Helm: `secrets.azureServiceBusConnectionString`).
  - **No secret, Microsoft Entra ID:** set `AZURE_SERVICE_BUS_NAMESPACE` (Helm: `config.azureServiceBusNamespace`) and give the pods' workload identity the *Azure Service Bus Data Owner* role. That role is needed to create entities; *Data Sender* + *Data Receiver* is enough when the entities already exist and `AutoCreateEntities` is false.

### Readiness

Readiness checks the namespace through the management API, so it needs *Manage* rights. With `AutoCreateEntities=false` (the emulator, or a send/listen-only policy) the check is skipped and reports healthy.

## Experiments

1. **Follow one ticket.** Create a ticket and watch the logs:
   - `Published TicketCreated … to Service Bus topic support.ticket-created`
   - then `… handled … (delivery 1, sequence N)` in the Ticket, Email, Notification and Audit services.
   - Creating a user does the same on `support.user-created`: three subscriptions (welcome email, welcome notification, audit), no saga.
2. **One copy per subscription.** In the Azure portal (real namespace) → topic → Subscriptions, each subscription's message count goes up and back to 0.
3. **Retries and dead letters.** Make a handler fail, for example with a bad connection string for `ss-notification-api`.
   - Watch deliveries 1–4 with growing delays, then "dead-lettering it".
   - Open the subscription's dead-letter queue in the portal's Service Bus Explorer and read the `DeadLetterReason` and `DeadLetterErrorDescription`.
4. **Competing consumers.** Run two instances of `SS.Email.API`.
   - Unlike Kafka, both receive messages from the same subscription, because there's no one-consumer-per-partition rule.
5. **Compare with Kafka.** Run the same experiments from [KAFKA.md](KAFKA.md).
   - Replay has no direct Service Bus equivalent: a completed message is gone.

## Not covered (yet)

- **Sessions**, for strict per-ticket ordering.
- **Duplicate detection** on topics (`RequiresDuplicateDetection`), which uses our stable MessageId.
- **Scheduled messages** (`ScheduledEnqueueTime`), useful for SLA-breach reminders.
