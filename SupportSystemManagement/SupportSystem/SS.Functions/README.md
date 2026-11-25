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

## Azure Function App

In Azure it runs as a real **Function App** on the Flex Consumption plan: Linux, .NET 8 isolated worker, billed per execution. It doesn't run inside AKS. [infra/function-app/main.bicep](../infra/function-app/main.bicep) creates:

| Resource | Purpose |
|---|---|
| Storage account | `AzureWebJobsStorage` (timer lease, host keys) and the `app-package` container the code zip is uploaded to |
| Log Analytics + Application Insights | logs and the function's invocation history |
| Flex Consumption plan + Function App | the app, with the settings from `local.settings.json` |

[scripts/deploy-function-app.sh](../scripts/deploy-function-app.sh) runs the Bicep, publishes the project, zips it and deploys the zip. Your machine and Jenkins both use this script.

> **Broker: Service Bus only.** The Function App runs outside the cluster, so it can't reach the in-cluster Kafka (Strimzi) or RabbitMQ. It always publishes to **Azure Service Bus**. The Notification and Audit services in AKS receive `TicketSlaBreached` only when the Helm release uses `config.messagingProvider: AzureServiceBus` (plus `secrets.azureServiceBusConnectionString`). With the default Kafka setup, the function still runs and marks tickets, but nobody receives the events.

> **Database: allow Azure services.** The app connects to the Azure SQL server from changing outbound IPs, so the server's networking must have *Allow Azure services and resources to access this server* switched on.

### Deploy from your machine

From `SupportSystemManagement/SupportSystem` in Git Bash, signed in with `az login`, with Contributor on the resource group:

```bash
export DB_CONNECTION_STRING='Server=...;Database=SupportSystem;...'
export AZURE_SERVICE_BUS_CONNECTION_STRING='Endpoint=sb://<namespace>.servicebus.windows.net/;SharedAccessKeyName=...;SharedAccessKey=...'
bash scripts/deploy-function-app.sh rg-supportsystem-functions centralindia
```

- **Location:** it's only needed the first time, when the script creates the resource group. Pick a region that offers Flex Consumption: `az functionapp list-flexconsumption-locations -o table`.
- **Code-only redeploy:** add `SKIP_INFRA=true FUNCTION_APP_NAME=<name>`.

### Deploy from Jenkins

Run the job with **ACTION = deploy** and tick **DEPLOY_FUNCTION_APP**. The *Deploy Function App* stage runs the same script and archives the zip with the build.

One-time setup:
- **Placeholder:** in the Jenkinsfile, replace `FUNCTION_APP_RESOURCE_GROUP = '<function-app-resource-group>'` with your resource group.
- **Credentials:** add two *Secret text* Jenkins credentials, `supportsystem-db-connection-string` and `supportsystem-servicebus-connection-string`.
- **Permissions:** the `azure-credentials-id` service principal needs Contributor on that resource group.
- **Agent:** it needs `az`, plus either the .NET 8 SDK or Docker; without `dotnet` the build runs in the `dotnet/sdk` container.

Jenkins checks out `master`, so the code must be merged before it can deploy it.

### Test it in Azure

1. **Make one ticket overdue**, as in local testing:
   ```sql
   UPDATE Tickets SET ResolutionDueDate = DATEADD(hour,-1,GETDATE()), SlaBreachedAt = NULL WHERE TicketId = <id>;
   ```
   This database is shared: every overdue, unreported ticket gets reported on the first run.
2. **Run it now** instead of waiting for the schedule: **Azure portal → Function App → Functions → SlaBreachChecker → Code + Test → Test/Run**. Or from the CLI:
   ```bash
   KEY=$(az functionapp keys list -g <rg> -n <app> --query masterKey -o tsv)
   curl -X POST "https://<app>.azurewebsites.net/admin/functions/SlaBreachChecker" -H "x-functions-key: $KEY" -H "Content-Type: application/json" -d "{}"
   ```
3. **Watch it run:** *Log stream* on the function page in the portal, or *Invocations* for past runs. You can also query Application Insights: `traces | where message has "SLA"`.
4. **Check the results:** `Tickets.SlaBreachedAt` is set, the Service Bus topic `support.ticket-sla-breached` has a message, and, when the APIs use Service Bus, the ⏰ notification and the audit-log row appear.

To speed up the schedule while testing, change the `SlaBreachCheckerSchedule` app setting in the portal (*Settings → Environment variables*). Changing it restarts the app, and the next deploy puts the Bicep value back.
