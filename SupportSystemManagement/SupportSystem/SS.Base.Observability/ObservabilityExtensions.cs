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
/// HttpClient, SQL, MassTransit/RabbitMQ) and metrics are also exported via OpenTelemetry to
/// Azure Monitor. Without one, nothing is exported and the service runs exactly as before.
///
/// Metrics are always exposed in Prometheus format at /metrics (see UseSupportSystemMetrics),
/// independent of Application Insights.
/// </summary>
public static class ObservabilityExtensions
{
    // ActivitySource/Meter name MassTransit emits its publish/consume/saga spans and metrics under.
    private const string MassTransitSource = "MassTransit";

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
            .WithTracing(t => t.AddSource(MassTransitSource))
            .WithMetrics(m => m
                .AddMeter(MassTransitSource)
                .AddMeter(BuiltInMeters)
                .AddRuntimeInstrumentation()   // GC, heap, thread pool, exceptions
                .AddPrometheusExporter());

        var connectionString = builder.Configuration["APPLICATIONINSIGHTS_CONNECTION_STRING"];
        if (!string.IsNullOrWhiteSpace(connectionString))
        {
            openTelemetry.UseAzureMonitor(o => o.ConnectionString = connectionString);
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
