using Microsoft.Extensions.DependencyInjection;

namespace SS.Base.Messaging.AzureServiceBus;

/// <summary>
/// Placeholder for the Azure Service Bus provider (a later phase). The planned shape, mirroring Kafka:
/// - one ASB topic per message type (same names as the Kafka topics),
/// - one subscription per handler (= Kafka consumer group), processed by a ServiceBusProcessor
///   that calls the same <c>IEventHandler&lt;T&gt;</c>s,
/// - ASB's built-in dead-letter sub-queue instead of a ".dlt" topic, MaxDeliveryCount for retries.
/// The existing user-created email queue (AzureServiceBusQueueSender / SS.Email.API's receiver) is
/// separate and untouched.
/// </summary>
internal static class AzureServiceBusRegistration
{
    public static void AddAzureServiceBusMessaging(this IServiceCollection services, MessagingBuilder builder) =>
        throw new NotSupportedException(
            "Messaging:Provider 'AzureServiceBus' isn't implemented yet. Use 'Kafka' or 'RabbitMq'.");
}
