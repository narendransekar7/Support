using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Ocelot.DependencyInjection;
using Ocelot.Middleware;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

// Load Ocelot configuration from ocelot.json

// Add configuration from environment variables
builder.Configuration.AddEnvironmentVariables();

string baseUrl = Environment.GetEnvironmentVariable("BaseUrl") ?? "http://localhost:5145";
// Add the resolved BaseUrl to the configuration
builder.Configuration["BaseUrl"] = baseUrl;
Console.WriteLine($"Resolved BaseUrl: {baseUrl}");

builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true).AddEnvironmentVariables();
builder.Services.AddOcelot(builder.Configuration);

// JWT Authentication using key which need to be check and removed in futher to access web api with out using this technique
var key = Encoding.ASCII.GetBytes("hldiSW6BAHCCzY9Yy1zQLiN+MHYJ0Fm5InfQlPANUyM=");
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(key),
            ValidateIssuer = false,
            ValidateAudience = false,
            ValidateLifetime = true
        };
    });





builder.Services.AddControllers();
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

app.UseAuthorization();
app.UseAuthorization();

app.MapControllers();


// Use Ocelot middleware
app.UseOcelot().Wait();

app.Run();
