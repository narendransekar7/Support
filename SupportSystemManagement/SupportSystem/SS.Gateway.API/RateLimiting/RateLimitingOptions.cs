namespace SS.Gateway.API.RateLimiting;

// Config section "RateLimiting" (env vars RateLimiting__*). The Redis connection itself is "Redis:ConnectionString".
public sealed class RateLimitingOptions
{
    public const string SectionName = "RateLimiting";

    public bool Enabled { get; set; } = true;

    // Prefix of every counter key in Redis, so the instance can be shared with other apps.
    public string KeyPrefix { get; set; } = "ss:ratelimit";

    // /api/auth/* (login, refresh-token, logout) - per client IP, kept low to slow down password guessing.
    public RateLimitPolicy Auth { get; set; } = new() { PermitLimit = 20, WindowSeconds = 60 };

    // Every other /api/* route - per signed-in user (Entra "oid" / password login "UserId"), per client IP otherwise.
    public RateLimitPolicy Api { get; set; } = new() { PermitLimit = 300, WindowSeconds = 60 };
}

public sealed class RateLimitPolicy
{
    public int PermitLimit { get; set; }
    public int WindowSeconds { get; set; }
}
