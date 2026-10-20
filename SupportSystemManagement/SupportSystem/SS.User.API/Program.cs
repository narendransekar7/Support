using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using SS.Base.Authentication;
using SS.Base.Application;
using SS.Base.Infrastructure.Persistance.MSSQL;
using SS.Base.Messaging;
using SS.Base.Observability;
using System;

var builder = WebApplication.CreateBuilder(args);

// Logging (stdout, JSON in containers) + OpenTelemetry export to Application Insights - see SS.Base.Observability.
builder.AddSupportSystemObservability("ss-user-api");

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
// React UI origin: https://localhost:44345 for local Visual Studio dev (Gateway's sslPort),
// http://localhost:5145 in Docker (Gateway container port 8080 mapped by docker-compose) —
// overridden there via Cors__ReactUIOrigin.
var reactUiOrigin = builder.Configuration["Cors:ReactUIOrigin"] ?? "https://localhost:44345";
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactUICorsPolicy", policy =>
    {
        policy.WithOrigins(reactUiOrigin)
            .AllowAnyHeader()                      // Allow all headers
            .AllowAnyMethod()                      // Allow all HTTP methods (GET, POST, etc.)
            .AllowCredentials();                   // Allow cookies and credentials
    });
});

// Same Support System token validation as SS.Gateway.API (either sign-in option) - only endpoints
// marked [Authorize] use it, e.g. /api/user/me, which needs the caller's identity from the token itself.
builder.Services.AddSupportSystemAuthentication(builder.Configuration);

//Added the extension method in the Application layer for AddMediatR
builder.Services.AddApplicationServices(builder.Configuration);


// Added the extension method in the infrastructure layer for SQL Server DB context
builder.Services.AddInfrastructureServices(builder.Configuration);

// Publisher only (no consumers here) on the broker chosen by Messaging:Provider - SS.Base.Application's
// MediatR handler scan registers CreateTicketHandler in every host that calls AddApplicationServices,
// and it depends on IEventPublisher.
builder.Services.AddSupportSystemMessaging(builder.Configuration, "ss-user-api");

// Kubernetes probes (see k8s/supportsystem.yaml): /health/live runs no checks so a broker outage
// doesn't restart the pod; /health/ready runs all registered checks (MassTransit's bus check or the Kafka check).
builder.Services.AddHealthChecks();

var app = builder.Build();

// Configure the HTTP request pipeline.
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

// Use CORS middleware
app.UseCors("ReactUICorsPolicy");

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
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
