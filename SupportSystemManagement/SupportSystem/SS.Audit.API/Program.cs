using MassTransit;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using SS.Base.Application;
using SS.Base.Application.Consumers;
using SS.Base.Application.EventHandlers;
using SS.Base.Authentication;
using SS.Base.Domain.Messages.Ticket;
using SS.Base.Domain.Messages.User;
using SS.Base.Infrastructure.Persistance.MSSQL;
using SS.Base.Messaging;
using SS.Base.Observability;

// SS.Audit.API: an append-only trail of what happened to tickets and users, built purely from bus messages
// (no other service calls it). Admins read it at /api/audit/* through the gateway.
var builder = WebApplication.CreateBuilder(args);

// Logging (stdout, JSON in containers) + OpenTelemetry export to Application Insights - see SS.Base.Observability.
builder.AddSupportSystemObservability("ss-audit-api");

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Same Support System token validation as the gateway; AuditController is Admin-only.
builder.Services.AddSupportSystemAuthentication(builder.Configuration);

builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

// Every fact of the ticket- and user-creation workflows is audited. Auditing another message = one more line in each list.
builder.Services.AddSupportSystemMessaging(builder.Configuration, "ss-audit-api", messaging =>
{
    messaging
        .Subscribe<TicketCreated, AuditEventHandler<TicketCreated>>()
        .Subscribe<EngineerAssigned, AuditEventHandler<EngineerAssigned>>()
        .Subscribe<AssignEngineerFailed, AuditEventHandler<AssignEngineerFailed>>()
        .Subscribe<SlaReserved, AuditEventHandler<SlaReserved>>()
        .Subscribe<UserCreated, AuditEventHandler<UserCreated>>();

    // RabbitMQ: one queue per audited message type, bound to that type's exchange.
    messaging.ForRabbitMq(x =>
    {
        x.AddConsumer<AuditConsumer<TicketCreated>>().Endpoint(e => e.Name = "audit-ticket-created");
        x.AddConsumer<AuditConsumer<EngineerAssigned>>().Endpoint(e => e.Name = "audit-engineer-assigned");
        x.AddConsumer<AuditConsumer<AssignEngineerFailed>>().Endpoint(e => e.Name = "audit-assign-engineer-failed");
        x.AddConsumer<AuditConsumer<SlaReserved>>().Endpoint(e => e.Name = "audit-sla-reserved");
        x.AddConsumer<AuditConsumer<UserCreated>>().Endpoint(e => e.Name = "audit-user-created");
    });
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready");

app.Run();
