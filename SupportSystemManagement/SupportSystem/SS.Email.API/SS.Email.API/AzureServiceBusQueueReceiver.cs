namespace SS.Email.API;

using Azure.Messaging.ServiceBus;
using System.Text.Json;
using System.Text;
using System.Threading.Tasks;
using SS.Base.Domain.Email;

public class AzureServiceBusQueueReceiver
{
    private readonly EmailService _emailService;
    private readonly string _connectionString;
    private readonly string _queueName;
    private readonly ServiceBusClient _serviceBusClient;

    public AzureServiceBusQueueReceiver(ServiceBusClient serviceBusClient,string connectionString, string queueName, EmailService emailService)
    {
        _connectionString = connectionString;
        _queueName = queueName;
        _emailService = emailService;
        _serviceBusClient = serviceBusClient;
    }

    public async Task ReceiveMessagesAsync()
    {
        //await using var client = new ServiceBusClient(_connectionString);
        //ServiceBusProcessor processor = client.CreateProcessor(_queueName, new ServiceBusProcessorOptions());
        ServiceBusProcessor processor = _serviceBusClient.CreateProcessor(_queueName, new ServiceBusProcessorOptions());

        processor.ProcessMessageAsync += async args =>
        {
            string body = args.Message.Body.ToString();
            var userCreatedMessage = JsonSerializer.Deserialize<UserCreatedMessage>(body);

            Console.WriteLine($"Received message for user: {userCreatedMessage.Email}");

            // Send Email
            string subject = "Welcome to Support System!";
            string emailBody = $"Hello {userCreatedMessage.FullName},<br/><br/>Your account has been successfully created!";

            await _emailService.SendEmailAsync(userCreatedMessage.Email, subject, emailBody);

            await args.CompleteMessageAsync(args.Message);
        };

        processor.ProcessErrorAsync += args =>
        {
            Console.WriteLine($"Error: {args.Exception.Message}");
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync();
    }
}