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
    private readonly ILogger<AzureServiceBusQueueReceiver> _logger;

    public AzureServiceBusQueueReceiver(ServiceBusClient serviceBusClient,string connectionString, string queueName, EmailService emailService, ILogger<AzureServiceBusQueueReceiver> logger)
    {
        _logger = logger;
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

            _logger.LogInformation("Received user-created message for user {UserId}", userCreatedMessage.UserId);

            // Send Email
            string subject = "Welcome to Support System!";
            string emailBody = $"Hello {userCreatedMessage.FullName},<br/><br/>Your account has been successfully created!";

            await _emailService.SendEmailAsync(userCreatedMessage.Email, subject, emailBody);

            await args.CompleteMessageAsync(args.Message);
        };

        processor.ProcessErrorAsync += args =>
        {
            _logger.LogError(args.Exception, "Service Bus error on {EntityPath} ({ErrorSource})", args.EntityPath, args.ErrorSource);
            return Task.CompletedTask;
        };

        await processor.StartProcessingAsync();
    }
}