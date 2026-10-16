using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using MassTransit;
using SS.Base.Application;
using SS.Base.Infrastructure.Persistance.MSSQL;
using SS.Base.Observability;

var builder = WebApplication.CreateBuilder(args);

// Logging (stdout, JSON in containers) + OpenTelemetry export to Application Insights - see SS.Base.Observability.
builder.AddSupportSystemObservability("ss-auth-server-api");

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Credential checks and refresh-token storage go through the shared Application handlers
// (ValidateUser / LoginSuccess / TokenRefresh / LogOut) against the Support System database.
builder.Services.AddApplicationServices(builder.Configuration);
builder.Services.AddInfrastructureServices(builder.Configuration);

// AddApplicationServices' MediatR scan also registers CreateTicketHandler, which depends on
// IPublishEndpoint. This host never publishes, so an in-memory bus satisfies that dependency
// without tying sign-in to RabbitMQ's availability.
builder.Services.AddMassTransit(x => x.UsingInMemory((context, cfg) => cfg.ConfigureEndpoints(context)));


// Kubernetes probes (see k8s/supportsystem.yaml): /health/live runs no checks so a RabbitMQ outage
// doesn't restart the pod; /health/ready runs all registered checks (incl. MassTransit's bus check).
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
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
