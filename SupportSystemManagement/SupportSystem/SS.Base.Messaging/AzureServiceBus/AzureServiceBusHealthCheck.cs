using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace SS.Base.Messaging.AzureServiceBus;

/// <summary>
/// Readiness: with management rights (AutoCreateEntities) reads the namespace properties, which needs a
/// working connection and credentials. Without them (emulator, send/listen-only policy) there's no
/// cheap call to make, so it only reports that the check was skipped.
/// </summary>
internal sealed class AzureServiceBusHealthCheck : IHealthCheck
{
    private readonly AzureServiceBusEntityManager _entities;

    public AzureServiceBusHealthCheck(AzureServiceBusEntityManager entities) => _entities = entities;

    public async Task<HealthCheckResult> CheckHealthAsync(HealthCheckContext context, CancellationToken cancellationToken = default)
    {
        if (!_entities.CanManage)
            return HealthCheckResult.Healthy("Not checked: AutoCreateEntities is off (no management access)");

        try
        {
            using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
            timeout.CancelAfter(TimeSpan.FromSeconds(5));
            var properties = await _entities.Admin.GetNamespacePropertiesAsync(timeout.Token);
            return HealthCheckResult.Healthy($"Service Bus namespace {properties.Value.Name} reachable");
        }
        catch (Exception ex) when (ex is ServiceBusException or OperationCanceledException or Azure.RequestFailedException or UnauthorizedAccessException)
        {
            return new HealthCheckResult(context.Registration.FailureStatus, "Service Bus unreachable", ex);
        }
    }
}
