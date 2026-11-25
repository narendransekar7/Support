using System.Diagnostics;
using Azure.Monitor.OpenTelemetry.AspNetCore;
using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Console;
using OpenTelemetry.Instrumentation.AspNetCore;
using OpenTelemetry.Metrics;
using OpenTelemetry.Resources;
using OpenTelemetry.Trace;

namespace SS.Base.Observability;

/// <summary>
/// One place every Support System host wires up logging, tracing and metrics.
///
/// Logs always go to stdout (what `docker logs` / `kubectl logs` read). The console format comes
/// from config: `Logging:Console:FormatterName` = "simple" (readable, local dev) or "json" (one
/// structured line per entry, set in docker-compose/k8s). Every entry carries the current
/// TraceId/SpanId as a scope, so stdout lines can be matched to a distributed trace.
///
/// When an Application Insights connection string is configured, logs, traces (ASP.NET Core,
/// HttpClient, SQL, MassTransit/RabbitMQ, Kafka) and metrics are also exported via OpenTelemetry to
/// Azure Monitor. Independently, when OTEL_EXPORTER_OTLP_ENDPOINT is set, traces are also exported over
/// OTLP - locally to Jaeger (docker-compose, http://localhost:16686) to see the time spent in each service,
/// HTTP hop, SQL query and Kafka/RabbitMQ message of a request. With neither, nothing is exported.
///
/// Metrics are always exposed in Prometheus format at /metrics (see UseSupportSystemMetrics),
/// independent of Application Insights.
/// </summary>
public static class ObservabilityExtensions
{
    // ActivitySource/Meter name MassTransit emits its publish/consume/saga spans and metrics under.
    private const string MassTransitSource = "MassTransit";

    // ActivitySource SS.Base.Messaging emits its Kafka publish/process spans under (MessagingTelemetry).
    private const string KafkaMessagingSource = "SS.Base.Messaging";

    // Wildcard for the services' own ActivitySources (SS.Ticket, SS.Gateway.RateLimiting, ...).
    private const string CustomSourcePrefix = "SS.*";

    // Meters built into .NET 8: HTTP server request duration/active requests, Kestrel connections,
    // outgoing HttpClient requests. Registered explicitly so they reach Prometheus even when the
    // Azure Monitor distro (which would otherwise add them) isn't enabled.
    private static readonly string[] BuiltInMeters =
    {
        "Microsoft.AspNetCore.Hosting",
        "Microsoft.AspNetCore.Server.Kestrel",
        "Microsoft.AspNetCore.Routing",
        "Microsoft.AspNetCore.Diagnostics",
        "System.Net.Http",
    };

    private const string MetricsPath = "/metrics";

    public static WebApplicationBuilder AddSupportSystemObservability(this WebApplicationBuilder builder, string serviceName)
    {
        builder.Logging.Configure(o =>
            o.ActivityTrackingOptions = ActivityTrackingOptions.TraceId | ActivityTrackingOptions.SpanId);
        builder.Services.Configure<SimpleConsoleFormatterOptions>(o =>
        {
            o.IncludeScopes = true;
            o.SingleLine = true;
            o.TimestampFormat = "HH:mm:ss ";
        });
        builder.Services.Configure<JsonConsoleFormatterOptions>(o =>
        {
            o.IncludeScopes = true;
            o.UseUtcTimestamp = true;
            o.TimestampFormat = "yyyy-MM-ddTHH:mm:ss.fffZ";
        });

        var openTelemetry = builder.Services.AddOpenTelemetry()
            // service.name becomes the cloud role name in Application Insights (Application Map node);
            // the instance id is the pod/container name so replicas can be told apart.
            .ConfigureResource(r => r.AddService(serviceName, serviceInstanceId: Environment.MachineName))
            .WithTracing(t => t.AddSource(MassTransitSource, KafkaMessagingSource))
            .WithMetrics(m => m
                .AddMeter(MassTransitSource)
                .AddMeter(BuiltInMeters)
                .AddRuntimeInstrumentation()   // GC, heap, thread pool, exceptions
                .AddPrometheusExporter());

        var connectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
        var azureMonitorEnabled = !string.IsNullOrWhiteSpace(connectionString);
        if (azureMonitorEnabled)
        {
            openTelemetry.UseAzureMonitor(o => o.ConnectionString = connectionString);
        }
        else
        {
            // The Azure Monitor distro brings its own ASP.NET Core / HttpClient / SQL instrumentation; adding it
            // again would record every span twice. Without the distro, add it here so traces still exist
            // (for the OTLP exporter below).
            openTelemetry.WithTracing(t => t
                .AddAspNetCoreInstrumentation(o => o.RecordException = true)
                .AddHttpClientInstrumentation(o => o.RecordException = true) // also Ocelot's downstream calls
                .AddSqlClientInstrumentation());                              // every EF Core query, with its duration
        }

        // Custom spans: any service can `new ActivitySource("SS.<Service>")` and wrap a slow-looking step in
        // `using var activity = source.StartActivity("...")` - it then shows up nested in the trace.
        openTelemetry.WithTracing(t => t.AddSource(CustomSourcePrefix));

        // OTLP export (Jaeger, Grafana Tempo, an OpenTelemetry Collector, the .NET Aspire dashboard, ...).
        // Set OTEL_EXPORTER_OTLP_ENDPOINT (e.g. http://jaeger:4317) to turn it on; the exporter reads it and the
        // other standard OTEL_EXPORTER_OTLP_* settings itself. Works alongside Application Insights.
        if (!string.IsNullOrWhiteSpace(builder.Configuration["OTEL_EXPORTER_OTLP_ENDPOINT"]))
        {
            openTelemetry.WithTracing(t => t.AddOtlpExporter());
        }

        // Kubernetes probes hit /health/* and Prometheus hits /metrics every few seconds per pod — don't trace them.
        builder.Services.Configure<AspNetCoreTraceInstrumentationOptions>(o =>
            o.Filter = context => !context.Request.Path.StartsWithSegments("/health")
                                  && !context.Request.Path.StartsWithSegments(MetricsPath));

        // With app.UseExceptionHandler(), unhandled exceptions are logged once and returned as a plain
        // RFC 7807 500 (no stack trace); the X-Trace-Id header identifies the failure.
        builder.Services.AddProblemDetails();

        return builder;
    }

    /// <summary>
    /// Serves the Prometheus scrape endpoint at /metrics. Registered as middleware (not an endpoint)
    /// so it also works ahead of Ocelot in the gateway, which is terminal.
    ///
    /// If `Metrics:Port` is configured (k8s sets it, with a matching Kestrel endpoint), /metrics is
    /// only answered on that port. Services expose only their app port, so metrics can't be read
    /// through the gateway's public LoadBalancer. Without it (local dev, docker-compose), /metrics
    /// is served on every port.
    /// </summary>
    public static IApplicationBuilder UseSupportSystemMetrics(this IApplicationBuilder app)
    {
        var configuration = app.ApplicationServices.GetRequiredService<IConfiguration>();
        var metricsPort = configuration.GetValue<int?>("Metrics:Port");

        return app.UseOpenTelemetryPrometheusScrapingEndpoint(context =>
            context.Request.Path == MetricsPath
            && (metricsPort == null || context.Connection.LocalPort == metricsPort));
    }

    /// <summary>
    /// Adds an X-Trace-Id response header so a failing request seen in the browser or in a bug
    /// report can be looked up directly in Application Insights / the JSON logs.
    /// </summary>
    public static IApplicationBuilder UseTraceIdResponseHeader(this IApplicationBuilder app)
    {
        return app.Use((context, next) =>
        {
            var traceId = Activity.Current?.TraceId.ToString();
            if (traceId != null)
            {
                context.Response.OnStarting(() =>
                {
                    context.Response.Headers["X-Trace-Id"] = traceId;
                    return Task.CompletedTask;
                });
            }
            return next(context);
        });
    }
}
