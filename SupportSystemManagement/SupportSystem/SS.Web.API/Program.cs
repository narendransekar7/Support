using SS.Base.Application;
using SS.Base.Infrastructure.Persistance.MSSQL;
using System;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();

// Configure CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("ReactUICorsPolicy", policy =>
    {
        policy.WithOrigins("https://localhost") // React UI origin
              .AllowAnyHeader()                      // Allow all headers
              .AllowAnyMethod()                      // Allow all HTTP methods (GET, POST, etc.)
              .AllowCredentials();                   // Allow cookies and credentials
    });
});


// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

//Added the extension method in the Application layer for AddMediatR
builder.Services.AddApplicationServices(builder.Configuration);


// Added the extension method in the infrastructure layer for SQL Server DB context
builder.Services.AddInfrastructureServices(builder.Configuration);



var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Use CORS middleware
app.UseCors("ReactUICorsPolicy");

app.UseAuthorization();

app.MapControllers();

app.Run();
