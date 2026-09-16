using MassTransit;
using SS.Base.Application;
using SS.Base.Application.Consumers;
using SS.Base.Application.Sagas;
using SS.Base.Domain.Entities;
using SS.Base.Infrastructure.Persistance.MSSQL;

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

// Ticket-creation saga (Assign Engineer -> Reserve SLA, with compensation) over RabbitMQ
builder.Services.AddMassTransit(x =>
{
    x.AddConsumer<AssignEngineerConsumer>();
    x.AddConsumer<ReserveSlaConsumer>();
    x.AddConsumer<CreateNotificationConsumer>();
    x.AddConsumer<DeleteTicketConsumer>();

    x.AddSagaStateMachine<TicketCreationStateMachine, TicketCreationSagaState>()
        .EntityFrameworkRepository(r =>
        {
            r.ExistingDbContext<MSSQLDbContext>();
            r.UseSqlServer();
        });

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

app.MapControllers();

app.Run();
