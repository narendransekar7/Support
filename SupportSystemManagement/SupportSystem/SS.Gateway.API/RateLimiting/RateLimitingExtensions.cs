using Microsoft.Extensions.Options;
using OpenTelemetry.Metrics;
using StackExchange.Redis;

namespace SS.Gateway.API.RateLimiting;

// Wires up the Redis-backed rate limiter - the per-request logic is in RateLimitingMiddleware.
public static class RateLimitingExtensions
{
    public const string MeterName = "SS.Gateway.RateLimiting";

    public static IServiceCollection AddSupportSystemRateLimiting(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<RateLimitingOptions>(configuration.GetSection(RateLimitingOptions.SectionName));
        services.ConfigureOpenTelemetryMeterProvider(m => m.AddMeter(MeterName));

        var connectionString = configuration["Redis:ConnectionString"];
        if (string.IsNullOrWhiteSpace(connectionString))
        {
            return services; // no Redis configured -> UseSupportSystemRateLimiting logs it and skips the middleware
        }

        services.AddSingleton<IConnectionMultiplexer>(_ =>
        {
            var options = ConfigurationOptions.Parse(connectionString);
            // Keep retrying in the background instead of failing start-up: the limiter fails open while Redis is away.
            options.AbortOnConnectFail = false;
            return ConnectionMultiplexer.Connect(options);
        });
        services.AddSingleton<RedisRateLimiter>();
        return services;
    }

    // Must run before Ocelot (terminal) - and after UseForwardedHeaders, so RemoteIpAddress is the real client.
    public static IApplicationBuilder UseSupportSystemRateLimiting(this IApplicationBuilder app)
    {
        var options = app.ApplicationServices.GetRequiredService<IOptions<RateLimitingOptions>>().Value;
        var logger = app.ApplicationServices.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(RateLimitingExtensions));
        var limiter = app.ApplicationServices.GetService<RedisRateLimiter>(); // also opens the Redis connection at start-up

        if (!options.Enabled || limiter is null)
        {
            logger.LogWarning("Rate limiting is disabled (RateLimiting:Enabled = {Enabled}, Redis configured = {RedisConfigured})",
                options.Enabled, limiter is not null);
            return app;
        }

        logger.LogInformation("Rate limiting enabled: auth {AuthLimit} req/{AuthWindow}s per IP, api {ApiLimit} req/{ApiWindow}s per user",
            options.Auth.PermitLimit, options.Auth.WindowSeconds, options.Api.PermitLimit, options.Api.WindowSeconds);

        return app.UseMiddleware<RateLimitingMiddleware>();
    }
}
