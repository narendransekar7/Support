using SS.Base.Application;
using SS.Base.Infrastructure.Persistance.MSSQL;
using System;
using SS.Web.API.Middlewares;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactUICorsPolicy", policy =>
    {
        policy.WithOrigins("http://ss-react-ui") // React UI origin
            .AllowAnyHeader()                      // Allow all headers
            .AllowAnyMethod()                      // Allow all HTTP methods (GET, POST, etc.)
            .AllowCredentials();                   // Allow cookies and credentials
    });
});

//Added the extension method in the Application layer for AddMediatR
builder.Services.AddApplicationServices(builder.Configuration);


// Added the extension method in the infrastructure layer for SQL Server DB context
builder.Services.AddInfrastructureServices(builder.Configuration);



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
