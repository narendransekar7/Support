using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using SS.Base.Authentication;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using SS.Base.Observability;

var builder = WebApplication.CreateBuilder(args);

// Logging (stdout, JSON in containers) + OpenTelemetry export to Application Insights - see SS.Base.Observability.
builder.AddSupportSystemObservability("ss-gateway-api");

// Add services to the container.

// Load Ocelot configuration from ocelot.json

// Add configuration from environment variables
builder.Configuration.AddEnvironmentVariables();

string baseUrl = Environment.GetEnvironmentVariable("BaseUrl") ?? "https://localhost:44345";
// Add the resolved BaseUrl to the configuration
builder.Configuration["BaseUrl"] = baseUrl;

// In Docker, downstream services are reached by container name over plain HTTP instead of
// localhost + dev-cert HTTPS ports, so a separate routing table is used for that environment.
var ocelotFile = builder.Environment.IsEnvironment("Docker") ? "ocelot.Docker.json" : "ocelot.json";
builder.Configuration.AddJsonFile(ocelotFile, optional: false, reloadOnChange: true).AddEnvironmentVariables();
builder.Services.AddOcelot(builder.Configuration);

// Bearer tokens from either sign-in option - Microsoft Entra ID (OpenID Connect, validated against the
// tenant's signing keys) or the SS.Auth.Server.API password login (Jwt:SigningKey) - see SS.Base.Authentication.
// The "Bearer" scheme name is what ocelot*.json routes reference as AuthenticationProviderKey.
builder.Services.AddSupportSystemAuthentication(builder.Configuration);





builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Kubernetes probes (see k8s/supportsystem.yaml): /health/live runs no checks so a RabbitMQ outage
// doesn't restart the pod; /health/ready runs all registered checks (incl. MassTransit's bus check).
builder.Services.AddHealthChecks();

var app = builder.Build();

app.Logger.LogInformation("Resolved BaseUrl: {BaseUrl}", baseUrl);

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseTraceIdResponseHeader();
// Prometheus /metrics - middleware, so it runs ahead of the terminal Ocelot pipeline.
app.UseSupportSystemMetrics();

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();


// Health endpoints registered as middleware ahead of Ocelot because
// Ocelot is terminal - a mapped endpoint would never be reached.
app.UseHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.UseHealthChecks("/health/ready");

// Use Ocelot middleware
app.UseOcelot().Wait();

app.Run();
