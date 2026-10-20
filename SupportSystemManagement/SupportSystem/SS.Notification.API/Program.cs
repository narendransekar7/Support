using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using SS.Base.Application;
using SS.Base.Application.Consumers;
using SS.Base.Application.EventHandlers;
using SS.Base.Domain.Messages.Ticket;
using SS.Base.Infrastructure.Persistance.MSSQL;
using SS.Base.Messaging;
using SS.Base.Observability;

// SS.Notification.API: owns in-app notifications. Writes them when tickets are created (consumer)
// and serves them to the SPA (/api/notification/*, through the gateway).
var builder = WebApplication.CreateBuilder(args);

// Logging (stdout, JSON in containers) + OpenTelemetry export to Application Insights - see SS.Base.Observability.
builder.AddSupportSystemObservability("ss-notification-api");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

// TicketCreated -> a Notification row for the ticket's creator, on the broker chosen by Messaging:Provider.
builder.Services.AddSupportSystemMessaging(builder.Configuration, "ss-notification-api", messaging =>
{
    messaging.Subscribe<TicketCreated, CreateNotificationHandler>();

    // RabbitMQ: same consumer (and queue, "CreateNotification") that used to run in SS.Ticket.API.
    messaging.ForRabbitMq(x => x.AddConsumer<CreateNotificationConsumer>());
});

// Kubernetes probes: /health/live runs no checks so a broker outage doesn't restart the pod;
// /health/ready runs all registered checks (MassTransit's bus check or the Kafka check).
builder.Services.AddHealthChecks();

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}
else
{
    app.UseExceptionHandler();
}

app.UseTraceIdResponseHeader();
app.UseSupportSystemMetrics();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready");

app.Run();
