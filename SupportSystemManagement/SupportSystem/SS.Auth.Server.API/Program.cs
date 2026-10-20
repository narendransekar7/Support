using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using SS.Base.Messaging;
using SS.Base.Application;
using SS.Base.Authentication;
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
// IEventPublisher. This host never publishes, so a no-op publisher satisfies that dependency
// without tying sign-in to any broker's availability (Kafka or RabbitMQ).
builder.Services.AddNoOpEventPublisher();

// "Sign in with Microsoft": validates the Entra ID access token that POST /api/auth/entra exchanges for a
// Support System session (config section "AzureAd") - see SS.Base.Authentication.
builder.Services.AddEntraIdTokenValidation(builder.Configuration);


// Kubernetes probes (see k8s/supportsystem.yaml): /health/live runs no checks; /health/ready runs all registered checks.
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

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready");

app.Run();
