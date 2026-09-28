using System.Diagnostics;
using MediatR;
using Microsoft.Extensions.Logging;

namespace SS.Base.Application.Behaviors;

/// <summary>
/// Logs every MediatR command/query by type name with its duration, and which ones fail.
/// Request payloads are intentionally never logged — several (ValidateUser, CreateUser, TokenRefresh)
/// carry passwords or refresh tokens.
/// </summary>
public class LoggingBehavior<TRequest, TResponse> : IPipelineBehavior<TRequest, TResponse>
{
    private readonly ILogger<LoggingBehavior<TRequest, TResponse>> _logger;

    public LoggingBehavior(ILogger<LoggingBehavior<TRequest, TResponse>> logger)
    {
        _logger = logger;
    }

    public async Task<TResponse> Handle(TRequest request, CancellationToken cancellationToken, RequestHandlerDelegate<TResponse> next)
    {
        var requestName = typeof(TRequest).Name;
        var stopwatch = Stopwatch.StartNew();
        try
        {
            var response = await next();
            _logger.LogInformation("Handled {RequestName} in {ElapsedMs} ms", requestName, stopwatch.ElapsedMilliseconds);
            return response;
        }
        catch (Exception ex)
        {
            // The exception itself (with stack) is logged once by the host's exception-handling middleware;
            // here we only record which use case failed and how long it ran.
            _logger.LogWarning("{RequestName} failed after {ElapsedMs} ms with {ExceptionType}", requestName, stopwatch.ElapsedMilliseconds, ex.GetType().Name);
            throw;
        }
    }
}
