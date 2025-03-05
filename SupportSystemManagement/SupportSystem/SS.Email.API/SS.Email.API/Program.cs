using Azure.Messaging.ServiceBus;
using SS.Email.API;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

/*
var config = builder.Configuration;

builder.Services.AddSingleton(new SS.Email.API.EmailService(
    config["SmtpSettings:Server"],
    int.Parse(config["SmtpSettings:Port"]),
    config["SmtpSettings:Username"],
    config["SmtpSettings:Password"],
    config["SmtpSettings:FromEmail"]
));



string connectionString = builder.Configuration["AzureServiceBus:ConnectionString"];
string queueName = builder.Configuration["AzureServiceBus:QueueName"];

var queueReceiver = new SS.Email.API.AzureServiceBusQueueReceiver(connectionString, queueName);
Task.Run(() => queueReceiver.ReceiveMessagesAsync());

*/

// Register dependencies
var config = builder.Configuration;
// Register ServiceBusClient as Singleton
builder.Services.AddSingleton<ServiceBusClient>(sp =>
    new ServiceBusClient(config["AzureServiceBus:ConnectionString"]));



builder.Services.AddSingleton<SS.Email.API.EmailService>(sp =>
    new SS.Email.API.EmailService(
        config["SmtpSettings:Server"],
        int.Parse(config["SmtpSettings:Port"]),
        config["SmtpSettings:Username"],
        config["SmtpSettings:Password"],
        config["SmtpSettings:FromEmail"]
    ));

builder.Services.AddSingleton<SS.Email.API.AzureServiceBusQueueReceiver>(sp =>
{
    var emailService = sp.GetRequiredService<SS.Email.API.EmailService>();
    var serviceBusClient = sp.GetRequiredService<ServiceBusClient>();
    string connectionString = builder.Configuration["AzureServiceBus:ConnectionString"];
    string queueName = builder.Configuration["AzureServiceBus:QueueName"];
    return new SS.Email.API.AzureServiceBusQueueReceiver(serviceBusClient,connectionString, queueName, emailService);
});

var app = builder.Build();

// Resolve and start queue receiver
var queueReceiver = app.Services.GetRequiredService<SS.Email.API.AzureServiceBusQueueReceiver>();
Task.Run(() => queueReceiver.ReceiveMessagesAsync());











// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

var summaries = new[]
{
    "Freezing", "Bracing", "Chilly", "Cool", "Mild", "Warm", "Balmy", "Hot", "Sweltering", "Scorching"
};

app.MapGet("/weatherforecast", () =>
    {
        var forecast = Enumerable.Range(1, 5).Select(index =>
                new WeatherForecast
                (
                    DateOnly.FromDateTime(DateTime.Now.AddDays(index)),
                    Random.Shared.Next(-20, 55),
                    summaries[Random.Shared.Next(summaries.Length)]
                ))
            .ToArray();
        return forecast;
    })
    .WithName("GetWeatherForecast")
    .WithOpenApi();

app.Run();

record WeatherForecast(DateOnly Date, int TemperatureC, string? Summary)
{
    public int TemperatureF => 32 + (int)(TemperatureC / 0.5556);
}