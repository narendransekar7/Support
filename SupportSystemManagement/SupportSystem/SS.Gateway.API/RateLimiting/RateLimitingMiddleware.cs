using System.Diagnostics.Metrics;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using SS.Base.Authentication;
using Microsoft.Extensions.Options;
using StackExchange.Redis;

namespace SS.Gateway.API.RateLimiting;

/// <summary>
/// Redis-backed rate limiting for every /api request that passes through the gateway.
///
///   /api/auth/*  -> "auth" policy, per client IP (slows down password guessing)
///   /api/*       -> "api" policy, per signed-in user, or per client IP for anonymous / invalid tokens
///
/// Over the limit, the request is answered with 429 + Retry-After and never reaches Ocelot or the
/// downstream APIs. Counters live in Redis so the limit holds across all gateway replicas.
///
/// Fails open: if Redis is down or slow, requests are let through (and counted in the metric below)
/// rather than taking the whole API down with it.
///
/// Registered by RateLimitingExtensions.UseSupportSystemRateLimiting. Like every convention-based
/// middleware it is created once, so it must not hold per-request state.
/// </summary>
public sealed class RateLimitingMiddleware
{
    private static readonly Meter Meter = new(RateLimitingExtensions.MeterName);

    // Prometheus: ss_gateway_rate_limit_requests_total{policy="api|auth", result="allowed|rejected|unavailable"}
    private static readonly Counter<long> Decisions =
        Meter.CreateCounter<long>("ss.gateway.rate_limit.requests", description: "Requests checked by the gateway rate limiter");

    private readonly RequestDelegate _next;
    private readonly RedisRateLimiter _limiter;
    private readonly RateLimitingOptions _options;
    private readonly ILogger<RateLimitingMiddleware> _logger;
    private long _lastUnavailableLogTicks;

    public RateLimitingMiddleware(
        RequestDelegate next,
        RedisRateLimiter limiter,
        IOptions<RateLimitingOptions> options,
        ILogger<RateLimitingMiddleware> logger)
    {
        _next = next;
        _limiter = limiter;
        _options = options.Value;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var path = context.Request.Path;
        if (!path.StartsWithSegments("/api"))
        {
            await _next(context); // UI assets, /health, /metrics
            return;
        }

        var isAuth = path.StartsWithSegments("/api/auth");
        var policyName = isAuth ? "auth" : "api";
        var policy = isAuth ? _options.Auth : _options.Api;
        var client = isAuth ? $"ip:{ClientIp(context)}" : await ClientKeyAsync(context);

        RateLimitDecision? decision = null;
        Exception? error = null;
        if (_limiter.IsAvailable)
        {
            try
            {
                decision = await _limiter.AcquireAsync($"{_options.KeyPrefix}:{policyName}:{client}", policy);
            }
            catch (Exception ex) when (ex is RedisException or RedisTimeoutException)
            {
                error = ex;
            }
        }

        if (decision is not { } result)
        {
            Decisions.Add(1, new("policy", policyName), new("result", "unavailable"));
            LogUnavailable(error);
            await _next(context);
            return;
        }

        Decisions.Add(1, new("policy", policyName), new("result", result.Allowed ? "allowed" : "rejected"));
        context.Response.Headers["X-RateLimit-Limit"] = result.Limit.ToString();
        context.Response.Headers["X-RateLimit-Remaining"] = result.Remaining.ToString();

        if (result.Allowed)
        {
            await _next(context);
            return;
        }

        var retryAfterSeconds = (int)Math.Ceiling(result.RetryAfter.TotalSeconds);
        _logger.LogWarning("Rate limit exceeded for {Policy} policy by {Client} on {Path}", policyName, client, path.Value);
        context.Response.Headers.RetryAfter = retryAfterSeconds.ToString();
        await Results.Problem(
            statusCode: StatusCodes.Status429TooManyRequests,
            title: "Too many requests",
            detail: $"Rate limit of {result.Limit} requests per {policy.WindowSeconds}s exceeded. Retry after {retryAfterSeconds}s.")
            .ExecuteAsync(context);
    }

    // Signed-in users get their own budget, so many users behind one NAT/proxy IP don't throttle each other.
    // AuthenticateAsync's result is cached for the request, so Ocelot's own token check later doesn't validate it twice.
    private static async Task<string> ClientKeyAsync(HttpContext context)
    {
        // Entra ID sends a Bearer header; the password login sends its token in the ss_access cookie.
        if (context.Request.Headers.Authorization.Count > 0 || context.Request.Cookies.ContainsKey(AuthCookies.AccessToken))
        {
            var result = await context.AuthenticateAsync(JwtBearerDefaults.AuthenticationScheme);
            var userId = result.Principal?.FindFirst("oid")?.Value        // Microsoft Entra ID
                         ?? result.Principal?.FindFirst("UserId")?.Value; // SS.Auth.Server.API password login
            if (!string.IsNullOrEmpty(userId))
            {
                return $"user:{userId}";
            }
        }
        return $"ip:{ClientIp(context)}";
    }

    private static string ClientIp(HttpContext context) => context.Connection.RemoteIpAddress?.ToString() ?? "unknown";

    // Redis down = one warning per 30s, not one per request.
    private void LogUnavailable(Exception? ex)
    {
        var now = DateTime.UtcNow.Ticks;
        var last = Interlocked.Read(ref _lastUnavailableLogTicks);
        if (now - last > TimeSpan.TicksPerSecond * 30 && Interlocked.CompareExchange(ref _lastUnavailableLogTicks, now, last) == last)
        {
            _logger.LogWarning(ex, "Redis unavailable - rate limiting is skipped (failing open) until it is back");
        }
    }
}
