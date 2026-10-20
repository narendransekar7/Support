# Azure Service Bus emulator config

`Config.json` declares every topic and subscription the services use when `Messaging:Provider` is
`AzureServiceBus`. The emulator has no management API, so the services can't create them (they run with
`Messaging:AzureServiceBus:AutoCreateEntities=false` against it). Against a real namespace they create
them themselves.

Keep it in sync with the `Subscribe<TMessage, THandler>()` calls in the hosts' `Program.cs`:

- topic = `support.<message-type-in-kebab-case>`, e.g. `TicketCreated` → `support.ticket-created`
- subscription = `<service-name>.<handler-in-kebab-case>` (generic arguments dropped), e.g.
  `ss-audit-api.audit-event-handler`
- `MaxDeliveryCount` = `Messaging:AzureServiceBus:MaxRetries` + 1 (4)

Used by `docker-compose.yml` (`--profile servicebus`) and `docker-compose.servicebus.yml`.
See [docs/messaging/AZURE-SERVICE-BUS.md](../docs/messaging/AZURE-SERVICE-BUS.md).
