using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using SS.Email.API;
using SS.Email.API.Consumers;
using SS.Email.API.Handlers;
using SS.Base.Domain.Messages.Ticket;
using SS.Base.Domain.Messages.User;
using SS.Base.Messaging;
using SS.Base.Observability;

var builder = WebApplication.CreateBuilder(args);

// Logging (stdout, JSON in containers) + OpenTelemetry export to Application Insights - see SS.Base.Observability.
builder.AddSupportSystemObservability("ss-email-api");

// Add services to the container.
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var config = builder.Configuration;

builder.Services.AddSingleton<SS.Email.API.EmailService>(sp =>
    new SS.Email.API.EmailService(
        config["SmtpSettings:Server"],
        int.Parse(config["SmtpSettings:Port"]),
        config["SmtpSettings:Username"],
        config["SmtpSettings:Password"],
        config["SmtpSettings:FromEmail"],
        sp.GetRequiredService<ILogger<SS.Email.API.EmailService>>()
    ));

// Welcome and ticket-creation emails, on the broker chosen by Messaging:Provider.
builder.Services.AddSupportSystemMessaging(builder.Configuration, "ss-email-api", messaging =>
{
    messaging
        .Subscribe<UserCreated, UserCreatedEmailHandler>()
        .Subscribe<TicketCreated, TicketCreatedEmailHandler>()
        .Subscribe<SendTicketCreationFailedEmail, TicketCreationFailedEmailHandler>();

    messaging.ForRabbitMq(x =>
    {
        x.AddConsumer<UserCreatedEmailConsumer>();
        x.AddConsumer<TicketCreatedEmailConsumer>();
        x.AddConsumer<TicketCreationFailedEmailConsumer>();
    });
});

// Kubernetes probes (see k8s/supportsystem.yaml): /health/live runs no checks so a broker outage
// doesn't restart the pod; /health/ready runs all registered checks (MassTransit's bus check or the Kafka check).
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseSupportSystemMetrics();

app.UseHttpsRedirection();

app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready");

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