# SS.Functions: Azure Functions (isolated worker)

Scheduled background jobs for the Support System. They run as an Azure Functions app, not as another ASP.NET API.

## SlaBreachChecker (timer trigger)

```
every SlaBreachCheckerSchedule (default "0 */5 * * * *" = every 5 min)
  └─ Tickets where Status != Closed AND ResolutionDueDate < now AND SlaBreachedAt IS NULL
       └─ for each: publish TicketSlaBreached  →  set Tickets.SlaBreachedAt
                         │
                         ├─ SS.Notification.API: notification for the assignee (creator if unassigned)
                         └─ SS.Audit.API:        audit-log row
```

- **Due dates** come from the ticket saga's "Reserve SLA" step (`ReserveSlaHandler`): High 1 day, Medium 3 days, Low 7 days.
- **Once per ticket.** `SlaBreachedAt` marks a ticket as reported. The event is published *before* the ticket is marked. If a run crashes between the two steps, the next run publishes again (at-least-once), so the breach is never lost.
- **Any broker.** The function publishes through `IEventPublisher`, so `Messaging__Provider` (Kafka / RabbitMq / AzureServiceBus) applies here just as it does in the APIs.
- **One run at a time.** The Functions host takes a blob lease in `AzureWebJobsStorage`, so a scaled-out app still runs each occurrence once.

## Run locally

1. Install [Azure Functions Core Tools v4](https://learn.microsoft.com/azure/azure-functions/functions-run-local) (`npm i -g azure-functions-core-tools@4`).
2. Start Azurite (the storage emulator timers need):
   `docker run -p 10000:10000 -p 10001:10001 -p 10002:10002 mcr.microsoft.com/azure-storage/azurite`
3. Copy `local.settings.example.json` to `local.settings.json` (git-ignored) and fill in the connection string.
4. Apply the `AddTicketSlaBreachedAt` migration once (from the solution root):
   `dotnet ef database update --project SS.Base.Infrastructure --startup-project SS.Base.Infrastructure`
5. Run `func start` in this folder.

To run it without waiting for the schedule:

```
curl -X POST http://localhost:7071/admin/functions/SlaBreachChecker -H "Content-Type: application/json" -d "{}"
```

To get a breach to test with, set a ticket's `ResolutionDueDate` to a past time in SQL. While you experiment, `SlaBreachCheckerSchedule = */30 * * * * *` runs the check every 30 seconds.

## Docker

`docker compose up ss-functions` also starts Azurite. The image is built on `mcr.microsoft.com/azure-functions/dotnet-isolated`, the same runtime Azure uses.

## Deploying to Azure (when you're ready)

Create a Function App (Flex Consumption, .NET 8 isolated) with its own storage account. Then set the same app settings as `local.settings.json`, using `__` separators (`Messaging__Provider`, `ConnectionStrings__DefaultConnection`, ...), and publish:
`func azure functionapp publish <app-name>`.
