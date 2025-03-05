namespace SS.Base.Application.Events;

using Azure.Messaging.ServiceBus;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using SS.Base.Domain.Email;

public class AzureServiceBusQueueSender
{
    private readonly string _connectionString;
    private readonly string _queueName;

    public AzureServiceBusQueueSender(string connectionString, string queueName)
    {
        _connectionString = connectionString;
        _queueName = queueName;
    }

    public async Task SendMessageAsync(UserCreatedMessage message)
    {
        await using var client = new ServiceBusClient(_connectionString);
        ServiceBusSender sender = client.CreateSender(_queueName);

        string messageBody = JsonSerializer.Serialize(message);
        ServiceBusMessage busMessage = new(messageBody)
        {
            ContentType = "application/json"
        };

        await sender.SendMessageAsync(busMessage);
    }
}