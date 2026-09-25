using Microsoft.AspNetCore.Diagnostics.HealthChecks;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Added to the
var userApiBaseUrl = builder.Configuration["UserApi:BaseUrl"] ?? "https://localhost:44335";
builder.Services.AddHttpClient("WebAPI", client =>
{
    client.BaseAddress = new Uri(userApiBaseUrl);
});


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

app.UseHttpsRedirection();

app.UseAuthorization();

app.MapControllers();
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready");

app.Run();
