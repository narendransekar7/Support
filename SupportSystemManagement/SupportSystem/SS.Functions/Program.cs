using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using SS.Functions;
using SS.Base.Infrastructure.Persistance.MSSQL;
using SS.Base.Messaging;

// SS.Functions: scheduled background jobs as Azure Functions (isolated worker). Unlike the *.API services this
// isn't a long-running web app: the Functions host starts this worker and calls a function when its trigger
// fires (here, a timer). On a Consumption / Flex plan it costs nothing between runs.
//
// It reuses the same building blocks as the APIs: the EF Core DbContext/repositories (SS.Base.Infrastructure)
// and IEventPublisher on the broker chosen by Messaging:Provider (SS.Base.Messaging) - so whatever it publishes
// reaches the other services exactly as if an API had published it.
var host = new HostBuilder()
    .ConfigureFunctionsWorkerDefaults()
    .ConfigureServices((context, services) =>
    {
        // Logs/telemetry to Application Insights when APPLICATIONINSIGHTS_CONNECTION_STRING is set.
        services.AddApplicationInsightsTelemetryWorkerService();
        services.ConfigureFunctionsApplicationInsights();

        services.AddInfrastructureServices(context.Configuration);

        // Publish only - no Subscribe calls, so this host consumes nothing.
        services.AddSupportSystemMessaging(context.Configuration, "ss-functions");

        services.AddOptions<SlaBreachCheckerOptions>()
            .Bind(context.Configuration.GetSection(SlaBreachCheckerOptions.SectionName));
    })
    .Build();

host.Run();
