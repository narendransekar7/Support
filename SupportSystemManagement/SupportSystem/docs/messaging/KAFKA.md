# Kafka in the Support System

This guide explains how the Support System uses Kafka, with pointers into the code and experiments
you can run. RabbitMQ (the original broker) and Azure Service Bus sit behind the same
switch, so you can compare them on the same workflow.

## The switch

```jsonc
"Messaging": {
  "Provider": "Kafka",                 // Kafka (default) | RabbitMq | AzureServiceBus
  "Kafka": { "BootstrapServers": "localhost:9092" }
}
```

- Code: [`SS.Base.Messaging/MessagingServiceCollectionExtensions.cs`](../../SS.Base.Messaging/MessagingServiceCollectionExtensions.cs).
- Each host calls `AddSupportSystemMessaging(config, "<service-name>", m => ...)`.
- Business code only ever sees two interfaces from `SS.Base.Domain/Interfaces/Messaging`:
  - `IEventPublisher.PublishAsync(message, key)`
  - `IEventHandler<T>.HandleAsync(message, context)`
- The provider decides how those become Kafka records, RabbitMQ exchanges or Service Bus topics.

Override the setting with `Messaging__Provider=RabbitMq`, or `MESSAGING_PROVIDER=RabbitMq docker compose up`.

## The workflow

```
Ticket API: CreateTicketHandler ──publish──▶ support.ticket-created  (3 partitions, key = TicketId)
                                                 │
     ┌───────────────────────────┬───────────────┼─────────────────────────┬────────────────────────────┐
 group ss-ticket-api.        group ss-email-api.   group ss-notification-api.   group ss-audit-api.
 ticket-creation-            ticket-created-       create-notification-         audit-event-handler.
 orchestrator.ticket-created email-handler         handler                      ticket-created
     │ (saga)                    │ email               │ Notification row           │ AuditLog row
     ▼
 support.assign-engineer-command ─▶ AssignEngineerHandler ─▶ support.engineer-assigned / support.assign-engineer-failed
 support.reserve-sla-command     ─▶ ReserveSlaHandler     ─▶ support.sla-reserved
 support.delete-ticket-command   ─▶ DeleteTicketHandler          (compensation)
 support.send-ticket-creation-failed-email ─▶ SS.Email.API       (compensation)
```

User creation is simpler, with no saga, just three independent consumer groups on one topic:

```
User API: CreateUserHandler ──publish──▶ support.user-created  (3 partitions, key = UserId)
                                                 │
            ┌────────────────────────────────────┼─────────────────────────────────────┐
 group ss-email-api.                 group ss-notification-api.            group ss-audit-api.
 user-created-email-handler          welcome-notification-handler          audit-event-handler.user-created
     │ welcome email                     │ Welcome notification (bell)         │ AuditLog row (UserId)
```

## Concepts, and where they live in the code

| Concept | What it means | In this project |
|---|---|---|
| **Topic** | A named, append-only log of records. | One topic per message type: `support.ticket-created`. Created at startup by `KafkaTopicManager`; the broker's auto-create is turned off. |
| **Partition** | A topic is split into ordered partitions. Order is guaranteed only within one partition. | 3 per topic (`Messaging:Kafka:Partitions`). |
| **Key** | Records with the same key always go to the same partition. | Key = ticket id (`KafkaEventPublisher`), so all of one ticket's records on a topic stay in order. |
| **Record value + headers** | The value is opaque bytes; headers are free-form metadata. | JSON value; headers `message-id`, `message-type`, `occurred-at`, `source`, `traceparent`. |
| **Producer acks / idempotence** | `acks=all` waits for every in-sync replica. Idempotence stops producer retries from writing a record twice. | `KafkaClientFactory.CreateProducer`. |
| **Consumer group** | Consumers sharing a `group.id` split the partitions between them. Each group gets every record. | One group per handler: `<service>.<handler>`, e.g. `ss-email-api.ticket-created-email-handler`. Email, notification, audit and the saga each get their own copy. |
| **Offset / commit** | Each group stores the next offset to read per partition. | Auto-commit is off. `KafkaConsumerWorker` commits **after** the handler succeeds, which gives at-least-once delivery. |
| **Rebalance** | When consumers join or leave a group, partitions are reassigned. | Logged as "assigned partitions" / "revoked partitions". |
| **At-least-once → idempotent handlers** | A crash between "handled" and "committed" means the record is read again. | `EventContext.EventId` (the `message-id` header) is stable. Audit (`AuditLogs.EventId` unique) and Notification (`SourceEventId` unique) skip duplicates. `AssignEngineerHandler` doesn't re-assign. |
| **Retries / dead-letter topic** | Kafka has neither built in; it's a client-side pattern. | A failing handler is retried `MaxRetries` (3) times with growing delays, then the record is copied to `<topic>.dlt` with `dlt-*` headers, and the consumer moves on. |
| **Retention / replay** | Records stay for the retention period whether or not they were read. | 7 days in docker-compose. A new group, or a group whose offsets you reset, re-reads history. |

## The saga on Kafka

RabbitMQ keeps the MassTransit state machine (`Sagas/TicketCreationStateMachine.cs`). With Kafka, the
same workflow runs as plain handlers in `Sagas/TicketCreationOrchestrator.cs`:

- State lives in the same `TicketCreationSagaStates` table, with the same state names.
- Saving state and publishing the next command are separate steps. If a record arrives and the saga is already in the state it would move to, the orchestrator re-sends that state's command; the steps are idempotent.
- Anything else unexpected is logged and ignored.
- `RowVersion` turns concurrent updates into a retry.

This is exactly the bookkeeping MassTransit does for you on RabbitMQ.

## Run it

**Full stack** (Kafka and RabbitMQ both run; the services use Kafka):

```bash
docker compose up -d --build
```

**Only Kafka**, with the services running from Visual Studio or `dotnet run` on `localhost:9092`:

```bash
docker compose -f docker-compose.kafka.yml up -d
```

- Kafka UI: <http://localhost:8085>
- App: <http://localhost> (or `INGRESS_PORT`)

The database needs the `AddAuditLogAndNotificationSourceEvent` migration. The design-time factory uses the connection string from `SS.User.API/appsettings.json`:

```bash
dotnet ef database update --project SS.Base.Infrastructure --startup-project SS.Base.Infrastructure
```

## Experiments

1. **Follow one ticket.** Create a ticket in the UI.
   - In Kafka UI → Topics → `support.ticket-created` → Messages, open the record and look at its key, partition and headers.
   - Then look at `support.assign-engineer-command`, `support.engineer-assigned`, `support.reserve-sla-command` and `support.sla-reserved`. All have the same key and the same partition number.
2. **Consumer groups.** Kafka UI → Consumers. Every group should show lag 0 after the ticket is processed.
3. **Lag and catch-up.** Run `docker compose stop ss-email-api` and create two tickets.
   - The `ss-email-api.*` group's lag goes to 2, while notification and audit keep up.
   - Run `docker compose start ss-email-api`. It reads the backlog from its committed offset, and lag returns to 0.
4. **Rebalance.** Start a second email consumer next to the container, on your machine.
   - It needs no database. In the compose stack, your machine reaches Kafka on `localhost:9094`.
   - Run it from `SS.Email.API/SS.Email.API`:

     ```bash
     Messaging__Kafka__BootstrapServers=localhost:9094 dotnet run --urls http://localhost:5990
     ```

   - Both instances log "assigned partitions"; the 3 partitions of each topic are now split between them.
   - Start a third and a fourth: one of them gets no partitions, because a group can't use more consumers than partitions.
   - Stop it, and its partitions move back.
5. **Replay.** Stop `ss-audit-api`, then in Kafka UI → Consumers → `ss-audit-api.audit-event-handler.ticket-created`, reset its offsets to the earliest.
   - Start it again. It re-reads every `TicketCreated`, and the audit handler skips them all as duplicates.
   - This shows why idempotency matters.
6. **Dead letters.** Make a handler throw, for example by stopping SQL Server or using a bad connection string for `ss-notification-api`.
   - The log shows attempts 1–4, then "moving it to support.ticket-created.dlt".
   - Open that topic and read the `dlt-reason` header.
7. **Switch providers.** Run `MESSAGING_PROVIDER=RabbitMq docker compose up -d`.
   - Same workflow, now visible in the RabbitMQ management UI (<http://localhost:15672>): one exchange per message type, one queue per consumer.

## RabbitMQ vs Kafka, side by side

| | RabbitMQ (MassTransit) | Kafka |
|---|---|---|
| Model | Queue: a message is removed once acked. | Log: records stay for the retention period; each group tracks its own position. |
| Fan-out | Exchange → one queue per consumer. | Topic → one consumer group per consumer. |
| Scaling a consumer | Competing consumers on one queue, any number. | At most one consumer per partition per group. |
| Ordering | Per queue, broken by competing consumers / retries. | Per partition, kept by keying (`TicketId`). |
| Replay | No (a message is gone after ack). | Yes (reset a group's offsets). |
| Retries / dead letters | Built into MassTransit (`_error` queues). | Built by hand here (`.dlt` topics). |
| Saga | MassTransit state machine. | Hand-written orchestrator. |

## Next steps

- **Transactional outbox:** write the message to an outbox table in the same DB transaction as the ticket, and relay it to Kafka. Today a failed publish leaves a ticket without a saga.
- **Schema evolution:** Avro/Protobuf with a schema registry instead of plain JSON.
- **Compare with Azure Service Bus:** the same workflow runs on Service Bus (`Messaging:Provider = AzureServiceBus`). Each Kafka topic is a Service Bus topic, each consumer group a subscription, and the `.dlt` topics are the built-in dead-letter sub-queue. See [AZURE-SERVICE-BUS.md](AZURE-SERVICE-BUS.md).
