using MassTransit;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SS.Base.Domain.Interfaces.Messaging;

namespace SS.Base.Messaging.RabbitMq;

/// <summary>
/// The original MassTransit-over-RabbitMQ setup (previously repeated in each host's Program.cs),
/// unchanged in behavior: the host's consumers/sagas from <see cref="MessagingBuilder.ForRabbitMq"/>,
/// convention-based endpoints, and the `RabbitMq` config section.
/// </summary>
internal static class RabbitMqRegistration
{
    public static void AddRabbitMqMessaging(this IServiceCollection services, IConfigurationSection rabbitMqConfig, MessagingBuilder builder)
    {
        services.AddMassTransit(x =>
        {
            foreach (var register in builder.RabbitMqRegistrations)
                register(x);

            x.UsingRabbitMq((context, cfg) =>
            {
                cfg.Host(rabbitMqConfig["Host"], rabbitMqConfig["VirtualHost"], h =>
                {
                    h.Username(rabbitMqConfig["Username"]);
                    h.Password(rabbitMqConfig["Password"]);
                });

                cfg.ConfigureEndpoints(context);
            });
        });

        services.AddScoped<IEventPublisher, MassTransitEventPublisher>();
    }
}
