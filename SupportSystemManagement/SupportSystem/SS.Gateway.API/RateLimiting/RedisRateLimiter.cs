using System.Globalization;
using StackExchange.Redis;

namespace SS.Gateway.API.RateLimiting;

public readonly record struct RateLimitDecision(bool Allowed, int Limit, int Remaining, TimeSpan RetryAfter);

/// <summary>
/// Sliding-window counter kept in Redis, so every gateway replica shares the same budget per client.
///
/// Each client has one counter per fixed window (e.g. per minute). A request is counted against
/// "current window + the part of the previous window that still overlaps the sliding window", which
/// avoids the 2x burst a plain fixed window allows at the window boundary while costing only two keys.
/// The check and the increment run in one Lua script, so concurrent requests on different pods can't
/// both slip under the limit.
/// </summary>
public sealed class RedisRateLimiter
{
    // KEYS[1] = current window counter, KEYS[2] = previous window counter
    // ARGV[1] = permit limit, ARGV[2] = weight of the previous window (0..1), ARGV[3] = window length in ms
    // Returns { allowed (1/0), requests still remaining in the sliding window }
    // (Lua numbers are truncated to integers on the way back, so the fractional math stays in the script.)
    private const string Script = """
        local limit = tonumber(ARGV[1])
        local current = tonumber(redis.call('GET', KEYS[1]) or '0')
        local previous = tonumber(redis.call('GET', KEYS[2]) or '0')
        local estimated = previous * tonumber(ARGV[2]) + current
        if estimated + 1 > limit then
          return { 0, 0 }
        end
        current = redis.call('INCR', KEYS[1])
        if current == 1 then
          redis.call('PEXPIRE', KEYS[1], tonumber(ARGV[3]) * 2)
        end
        return { 1, math.floor(limit - estimated - 1) }
        """;

    private readonly IConnectionMultiplexer _redis;

    public RedisRateLimiter(IConnectionMultiplexer redis) => _redis = redis;

    public bool IsAvailable => _redis.IsConnected;

    public async Task<RateLimitDecision> AcquireAsync(string key, RateLimitPolicy policy)
    {
        var windowMs = policy.WindowSeconds * 1000L;
        var nowMs = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var window = nowMs / windowMs;
        var elapsedInWindowMs = nowMs - window * windowMs;
        var previousWeight = 1 - (double)elapsedInWindowMs / windowMs;

        // "{key}" is a hash tag: both counters land in the same slot, as Lua scripts require on a Redis Cluster.
        var result = (RedisResult[])(await _redis.GetDatabase().ScriptEvaluateAsync(
            Script,
            new RedisKey[] { $"{{{key}}}:{window}", $"{{{key}}}:{window - 1}" },
            new RedisValue[]
            {
                policy.PermitLimit,
                previousWeight.ToString("0.####", CultureInfo.InvariantCulture),
                windowMs
            }))!;

        var allowed = (int)result[0] == 1;
        var remaining = (int)result[1];
        // Rejected: by the end of the current window at the latest, the counter has room again.
        var retryAfter = allowed ? TimeSpan.Zero : TimeSpan.FromMilliseconds(windowMs - elapsedInWindowMs);

        return new RateLimitDecision(allowed, policy.PermitLimit, remaining, retryAfter);
    }
}
