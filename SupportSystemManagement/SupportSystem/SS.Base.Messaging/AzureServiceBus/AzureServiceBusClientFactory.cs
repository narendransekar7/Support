using Azure.Identity;
using Azure.Messaging.ServiceBus;
using Azure.Messaging.ServiceBus.Administration;
using Microsoft.Extensions.Options;

namespace SS.Base.Messaging.AzureServiceBus;

/// <summary>
/// Builds the Service Bus clients from either the connection string or the namespace + DefaultAzureCredential.
/// </summary>
internal sealed class AzureServiceBusClientFactory
{
    private readonly AzureServiceBusOptions _options;
    private readonly MessagingServiceName _serviceName;

    public AzureServiceBusClientFactory(IOptions<AzureServiceBusOptions> options, MessagingServiceName serviceName)
    {
        _options = options.Value;
        _serviceName = serviceName;
    }

    public ServiceBusClient CreateClient()
    {
        var clientOptions = new ServiceBusClientOptions
        {
            Identifier = _serviceName.Value,
            TransportType = _options.UseWebSockets ? ServiceBusTransportType.AmqpWebSockets : ServiceBusTransportType.AmqpTcp,
        };

        return !string.IsNullOrWhiteSpace(_options.ConnectionString)
            ? new ServiceBusClient(_options.ConnectionString, clientOptions)
            : new ServiceBusClient(_options.FullyQualifiedNamespace, new DefaultAzureCredential(), clientOptions);
    }

    public ServiceBusAdministrationClient CreateAdministrationClient() =>
        !string.IsNullOrWhiteSpace(_options.ConnectionString)
            ? new ServiceBusAdministrationClient(_options.ConnectionString)
            : new ServiceBusAdministrationClient(_options.FullyQualifiedNamespace, new DefaultAzureCredential());
}
