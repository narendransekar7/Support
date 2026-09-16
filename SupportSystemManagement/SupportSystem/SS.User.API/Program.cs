using MassTransit;
using SS.Base.Application;
using SS.Base.Infrastructure.Persistance.MSSQL;
using System;
using SS.User.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

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

//Added the extension method in the Application layer for AddMediatR
builder.Services.AddApplicationServices(builder.Configuration);


// Added the extension method in the infrastructure layer for SQL Server DB context
builder.Services.AddInfrastructureServices(builder.Configuration);

// Bus-only MassTransit registration (no consumers here) — SS.Base.Application's
// MediatR handler scan registers CreateTicketHandler in every host that calls
// AddApplicationServices, and it depends on IPublishEndpoint. This host doesn't
// consume anything, it just needs the bus present so that dependency resolves.
builder.Services.AddMassTransit(x =>
{
    x.UsingRabbitMq((context, cfg) =>
    {
        var rabbitMqConfig = builder.Configuration.GetSection("RabbitMq");
        cfg.Host(rabbitMqConfig["Host"], rabbitMqConfig["VirtualHost"], h =>
        {
            h.Username(rabbitMqConfig["Username"]);
            h.Password(rabbitMqConfig["Password"]);
        });

        cfg.ConfigureEndpoints(context);
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS middleware
app.UseCors("ReactUICorsPolicy");

app.UseAuthorization();

app.UseMiddleware<ApiKeyMiddleware>();

app.MapControllers();

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
